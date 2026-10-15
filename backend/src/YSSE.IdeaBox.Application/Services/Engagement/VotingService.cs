using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class VotingService : IVotingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public VotingService(IUnitOfWork db, ICurrentUserService current, IDateTimeProvider clock, IAuditWriter audit)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
    }

    public async Task<VoteResultDto> GetVoteStateAsync(int ideaId, CancellationToken ct = default)
    {
        var period = await _unitOfWork.Repository<VotingPeriod>().Query()
            .FirstOrDefaultAsync(vp => vp.IsActive && !vp.IsClosed, ct);

        var voted = _current.IsAuthenticated && _current.UserId is not null &&
                    await _unitOfWork.Repository<IdeaVote>().Query().AnyAsync(v => v.IdeaId == ideaId &&
                                                      v.UserId == _current.UserId &&
                                                      !v.IsDeleted && v.IsCounted, ct);

        var voteCount = await _unitOfWork.Repository<IdeaVote>().Query().CountAsync(v => v.IdeaId == ideaId && !v.IsDeleted && v.IsCounted, ct);

        var countIsVisible = period is null || period.Visibility == VoteVisibility.Live ||
                             period.IsClosed || _current.IsAdmin || !period.IsActive;

        return new VoteResultDto
        {
            IdeaId = ideaId,
            Voted = voted,
            VoteCount = countIsVisible ? voteCount : 0,
            CountIsVisible = countIsVisible
        };
    }

    public async Task<Result<VoteResultDto>> CastAsync(int ideaId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (idea.Status is not (IdeaStatus.Published or IdeaStatus.Shortlisted))
        {
            return Result<VoteResultDto>.Fail("Only published or shortlisted ideas can receive votes.");
        }

        if (idea.CreatorUserId == userId)
        {
            throw new ForbiddenException("You cannot vote on your own idea.");
        }

        var period = await _unitOfWork.Repository<VotingPeriod>().Query()
            .Where(vp => vp.IsActive && !vp.IsClosed)
            .OrderByDescending(vp => vp.StartAt)
            .FirstOrDefaultAsync(ct);
        if (period is null)
        {
            return Result<VoteResultDto>.Fail("Voting is not open right now.");
        }

        var now = _clock.UtcNow;
        if (now < period.StartAt || now > period.EndAt)
        {
            return Result<VoteResultDto>.Fail("The voting window is not currently open.");
        }

        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new UnauthorizedException();
        }

        if (user.IsSuspended)
        {
            throw new ForbiddenException("Your account is suspended.");
        }

        if (period.RequiresVerifiedAccount && !user.IsEmailVerified && !user.IsPhoneVerified)
        {
            return Result<VoteResultDto>.Fail("A verified account is required to vote.");
        }

        if (user.CreatedAt > now.AddDays(-period.MinimumAccountAgeDays))
        {
            return Result<VoteResultDto>.Fail($"Your account must be at least {period.MinimumAccountAgeDays} days old to vote.");
        }

        var existing = await _unitOfWork.Repository<IdeaVote>().Query().FirstOrDefaultAsync(
            v => v.IdeaId == ideaId && v.UserId == userId && v.VotingPeriodId == period.Id && !v.IsDeleted, ct);
        if (existing is not null)
        {
            return Result<VoteResultDto>.Ok(await BuildResultAsync(ideaId, true, period, ct));
        }

        await _unitOfWork.Repository<IdeaVote>().AddAsync(new IdeaVote
        {
            IdeaId = ideaId,
            UserId = userId,
            VotingPeriodId = period.Id,
            IsCounted = true,
            CreatedAt = now
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<VoteResultDto>.Ok(await BuildResultAsync(ideaId, true, period, ct));
    }

    public async Task<Result<VoteResultDto>> RemoveAsync(int ideaId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var vote = await _unitOfWork.Repository<IdeaVote>().Query().FirstOrDefaultAsync(
            v => v.IdeaId == ideaId && v.UserId == userId && !v.IsDeleted, ct);
        if (vote is null)
        {
            return Result<VoteResultDto>.Fail("You have not voted on this idea.");
        }

        vote.IsDeleted = true;
        vote.DeletedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var period = await _unitOfWork.Repository<VotingPeriod>().Query().FirstOrDefaultAsync(vp => vp.Id == vote.VotingPeriodId, ct);
        return Result<VoteResultDto>.Ok(new VoteResultDto
        {
            IdeaId = ideaId,
            Voted = false,
            VoteCount = CountsVisible(period) ? await CountVotesAsync(ideaId, ct) : 0,
            CountIsVisible = CountsVisible(period)
        });
    }

    public async Task<IReadOnlyList<VotingPeriodDto>> GetPeriodsAsync(CancellationToken ct = default)
    {
        var periods = await _unitOfWork.Repository<VotingPeriod>().Query()
            .OrderByDescending(vp => vp.StartAt)
            .Select(vp => new VotingPeriodDto
            {
                Id = vp.Id,
                Name = vp.Name,
                StartAt = vp.StartAt,
                EndAt = vp.EndAt,
                Visibility = vp.Visibility,
                IsActive = vp.IsActive,
                IsClosed = vp.IsClosed
            })
            .ToListAsync(ct);

        if (_current.IsAdmin)
        {
            return periods;
        }

        var now = _clock.UtcNow;
        return periods
            .Where(p => p.Visibility == VoteVisibility.Live || p.IsClosed || now > p.EndAt)
            .ToList();
    }

    public async Task<Result<int>> CreatePeriodAsync(VotingPeriodCreateRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        if (request.EndAt <= request.StartAt)
        {
            return Result<int>.Fail("The period must end after it starts.");
        }

        var period = new VotingPeriod
        {
            Name = request.Name.Trim(),
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Visibility = request.Visibility,
            MinimumAccountAgeDays = request.MinimumAccountAgeDays,
            RequiresVerifiedAccount = request.RequiresVerifiedAccount,
            IsActive = request.IsActive,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<VotingPeriod>().AddAsync(period, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("VotingPeriodCreated", nameof(VotingPeriod), period.Id.ToString(),
            null, new { period.Name, period.StartAt, period.EndAt }, ct);
        return Result<int>.Ok(period.Id);
    }

public async Task<Result> SetPeriodActiveAsync(int id, bool active, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var period = await _unitOfWork.Repository<VotingPeriod>().Query().FirstOrDefaultAsync(vp => vp.Id == id, ct);
        if (period is null)
        {
            throw new NotFoundException("Voting period not found.");
        }

        if (period.IsClosed)
        {
            return Result.Fail("A closed voting period cannot be reactivated.");
        }

        period.IsActive = active;
        period.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("VotingPeriodActiveChanged", nameof(VotingPeriod), period.Id.ToString(),
            null, new { active }, ct);
        return Result.Ok();
    }

    public async Task<Result> UpdatePeriodAsync(int id, VotingPeriodCreateRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var period = await _unitOfWork.Repository<VotingPeriod>().Query().FirstOrDefaultAsync(vp => vp.Id == id, ct);
        if (period is null)
        {
            throw new NotFoundException("Voting period not found.");
        }

        if (request.EndAt <= request.StartAt)
        {
            return Result.Fail("The period must end after it starts.");
        }

        period.Name = request.Name.Trim();
        period.StartAt = request.StartAt;
        period.EndAt = request.EndAt;
        period.Visibility = request.Visibility;
        period.MinimumAccountAgeDays = request.MinimumAccountAgeDays;
        period.RequiresVerifiedAccount = request.RequiresVerifiedAccount;
        period.IsActive = request.IsActive;
        period.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("VotingPeriodUpdated", nameof(VotingPeriod), period.Id.ToString(),
            null, new { period.Name, period.StartAt, period.EndAt }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeletePeriodAsync(int id, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var period = await _unitOfWork.Repository<VotingPeriod>().Query().FirstOrDefaultAsync(vp => vp.Id == id, ct);
        if (period is null)
        {
            throw new NotFoundException("Voting period not found.");
        }

        var hasVotes = await _unitOfWork.Repository<IdeaVote>().Query().AnyAsync(v => v.VotingPeriodId == id && !v.IsDeleted, ct);
        if (hasVotes)
        {
            return Result.Fail("This voting period has recorded votes and cannot be deleted.");
        }

        _unitOfWork.Repository<VotingPeriod>().Remove(period);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("VotingPeriodDeleted", nameof(VotingPeriod), period.Id.ToString(),
            null, new { period.Name }, ct);
        return Result.Ok();
    }

    public async Task<Result> ClosePeriodAsync(int id, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var period = await _unitOfWork.Repository<VotingPeriod>().Query().FirstOrDefaultAsync(vp => vp.Id == id, ct);
        if (period is null)
        {
            throw new NotFoundException("Voting period not found.");
        }

        period.IsClosed = true;
        period.IsActive = false;
        period.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("VotingPeriodClosed", nameof(VotingPeriod), period.Id.ToString(), null, null, ct);
        return Result.Ok();
    }

    private async Task<VoteResultDto> BuildResultAsync(int ideaId, bool voted, VotingPeriod period, CancellationToken ct)
    {
        var visible = CountsVisible(period);
        return new VoteResultDto
        {
            IdeaId = ideaId,
            Voted = voted,
            VoteCount = visible ? await CountVotesAsync(ideaId, ct) : 0,
            CountIsVisible = visible
        };
    }

    private bool CountsVisible(VotingPeriod? period) =>
        period is null || period.Visibility == VoteVisibility.Live || period.IsClosed || _current.IsAdmin;

    private async Task<int> CountVotesAsync(int ideaId, CancellationToken ct) =>
        await _unitOfWork.Repository<IdeaVote>().Query().CountAsync(v => v.IdeaId == ideaId && !v.IsDeleted && v.IsCounted, ct);
}


