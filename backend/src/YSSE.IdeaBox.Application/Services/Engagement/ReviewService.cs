using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public ReviewService(
        IUnitOfWork db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit,
        INotificationDispatcher notifications)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<ReviewCountsDto> GetCountsAsync(int ideaId, CancellationToken ct = default)
    {
        var published = await _unitOfWork.Repository<IdeaReview>().Query()
            .Where(r => r.IdeaId == ideaId && !r.IsDeleted && r.ModerationStatus == ReviewModerationStatus.Published)
            .GroupBy(r => r.IsGuest)
            .Select(g => new { IsGuest = g.Key, Count = g.Count(), AvgRating = g.Average(r => (double?)r.Rating) })
            .ToListAsync(ct);

        var anonymous = published.FirstOrDefault(x => x.IsGuest);
        var registered = published.FirstOrDefault(x => !x.IsGuest);
        var ratings = await _unitOfWork.Repository<IdeaReview>().Query()
            .Where(r => r.IdeaId == ideaId && !r.IsDeleted && r.ModerationStatus == ReviewModerationStatus.Published && r.Rating != null)
            .Select(r => (double?)r.Rating)
            .ToListAsync(ct);

        return new ReviewCountsDto
        {
            AnonymousPublished = anonymous?.Count ?? 0,
            RegisteredPublished = registered?.Count ?? 0,
            AverageRating = ratings.Count == 0 ? null : ratings.Average()
        };
    }

    public async Task<PagedResult<ReviewDto>> GetForIdeaAsync(int ideaId, int page, int pageSize, CancellationToken ct = default)
    {
        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<IdeaReview>().Query().Where(r => r.IdeaId == ideaId && !r.IsDeleted && r.ModerationStatus == ReviewModerationStatus.Published);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                Body = r.Body,
                Rating = r.Rating,
                IsGuest = r.IsGuest,
                ModerationStatus = r.ModerationStatus,
                DisplayName = "Anonymous",
                CreatedAt = r.CreatedAt,
                CanEdit = r.ReviewerUserId != null && r.ReviewerUserId == _current.UserId
            })
            .ToListAsync(ct);

        return PagedResult<ReviewDto>.Create(items, p, s, total);
    }

    public async Task<PagedResult<MyReviewDto>> GetMineAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<IdeaReview>().Query().Where(r => r.ReviewerUserId == userId && !r.IsDeleted);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(r => r.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(r => new MyReviewDto
            {
                Id = r.Id,
                IdeaId = r.IdeaId,
                IdeaSlug = r.Idea.Slug,
                IdeaTitle = r.Idea.Title,
                Status = r.Idea.Status,
                Body = r.Body,
                Rating = r.Rating,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<MyReviewDto>.Create(items, p, s, total);
    }

    public async Task<Result<ReviewDto>> SubmitRegisteredAsync(int ideaId, ReviewCreateRequest request, CancellationToken ct = default)    {
        var userId = _current.RequireUserId();
        var idea = await RequirePublishableIdeaAsync(ideaId, ct);
        if (idea.CreatorUserId == userId)
        {
            throw new ForbiddenException("You cannot review your own idea.");
        }

        var existing = await _unitOfWork.Repository<IdeaReview>().Query().FirstOrDefaultAsync(
            r => r.IdeaId == ideaId && r.ReviewerUserId == userId && !r.IsDeleted, ct);
        if (existing is not null)
        {
            existing.Body = request.Body;
            existing.Rating = request.Rating;
            existing.UpdatedAt = _clock.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<ReviewDto>.Ok(ToDto(existing, _current.UserId));
        }

        var review = new IdeaReview
        {
            IdeaId = ideaId,
            ReviewerUserId = userId,
            IsGuest = false,
            Body = request.Body,
            Rating = request.Rating,
            ModerationStatus = ReviewModerationStatus.Published,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<IdeaReview>().AddAsync(review, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(idea.CreatorUserId, NotificationType.NewReview,
            "New review on your idea", $"Someone left a review on \"{idea.Title}\".", null, ct: ct);

        return Result<ReviewDto>.Ok(ToDto(review, userId));
    }

    public async Task<Result<ReviewDto>> SubmitGuestAsync(int ideaId, ReviewCreateRequest request, GuestContext guest, CancellationToken ct = default)
    {
        if (request.Body.Trim().Length < 10)
        {
            return Result<ReviewDto>.Fail("Review must be at least 10 characters.");
        }

        var idea = await RequirePublishableIdeaAsync(ideaId, ct);

        var guestHash = !string.IsNullOrWhiteSpace(guest.GuestIdHash)
            ? guest.GuestIdHash
            : (!string.IsNullOrWhiteSpace(guest.RawGuestId) ? guest.RawGuestId : throw new UnauthorizedException("A guest identity is required."));

        var visitor = await _unitOfWork.Repository<GuestVisitor>().Query().FirstOrDefaultAsync(g => g.GuestIdHash == guestHash, ct);

        if (visitor is null)
        {
            visitor = new GuestVisitor
            {
                GuestIdHash = guestHash,
                IpHash = guest.IpHash,
                FirstReviewIdeaId = ideaId,
                FirstSeenAt = _clock.UtcNow,
                LastSeenAt = _clock.UtcNow
            };
            await _unitOfWork.Repository<GuestVisitor>().AddAsync(visitor, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        else if (visitor.FirstReviewIdeaId != ideaId)
        {
            throw new ForbiddenException("Please sign in to review another idea.");
        }

        var existing = await _unitOfWork.Repository<IdeaReview>().Query().FirstOrDefaultAsync(
            r => r.IdeaId == ideaId && r.GuestVisitorId == visitor.Id && !r.IsDeleted, ct);
        if (existing is not null)
        {
            existing.Body = request.Body;
            existing.Rating = request.Rating;
            existing.UpdatedAt = _clock.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<ReviewDto>.Ok(ToDto(existing, null));
        }

        if (visitor.IpHash is null)
        {
            visitor.IpHash = guest.IpHash;
        }

        var review = new IdeaReview
        {
            IdeaId = ideaId,
            GuestVisitorId = visitor.Id,
            IsGuest = true,
            Body = request.Body,
            Rating = request.Rating,
            ModerationStatus = ReviewModerationStatus.Published,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<IdeaReview>().AddAsync(review, ct);
        visitor.LastSeenAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(idea.CreatorUserId, NotificationType.NewReview,
            "New anonymous review", $"An anonymous visitor left a review on \"{idea.Title}\".", null, ct: ct);

        return Result<ReviewDto>.Ok(ToDto(review, null));
    }

    public async Task<Result<ReviewDto>> EditOwnAsync(int reviewId, ReviewCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var review = await _unitOfWork.Repository<IdeaReview>().Query().FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted, ct);
        if (review is null || review.ReviewerUserId != userId)
        {
            throw new NotFoundException("Review not found.");
        }

        var previous = review.Body;
        review.Body = request.Body;
        review.Rating = request.Rating;
        review.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("ReviewEdited", nameof(IdeaReview), review.Id.ToString(),
            null, new { previous }, ct);
        return Result<ReviewDto>.Ok(ToDto(review, userId));
    }

    public async Task<Result> DeleteOwnAsync(int reviewId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var review = await _unitOfWork.Repository<IdeaReview>().Query().FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted, ct);
        if (review is null || review.ReviewerUserId != userId)
        {
            throw new NotFoundException("Review not found.");
        }

        review.IsDeleted = true;
        review.DeletedAt = _clock.UtcNow;
        review.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync("ReviewDeleted", nameof(IdeaReview), review.Id.ToString(), null, null, ct);
        return Result.Ok();
    }

    public async Task<Result> ModerateAsync(int reviewId, ReviewModerateRequest request, CancellationToken ct = default)
    {
        if (!_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        var review = await _unitOfWork.Repository<IdeaReview>().Query().FirstOrDefaultAsync(r => r.Id == reviewId, ct);
        if (review is null)
        {
            throw new NotFoundException("Review not found.");
        }

        review.ModerationStatus = request.Status;
        review.ModerationReason = request.Reason;
        review.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("ReviewModerated", nameof(IdeaReview), review.Id.ToString(),
            request.Reason, new { request.Status }, ct);
        return Result.Ok();
    }

    private async Task<Idea> RequirePublishableIdeaAsync(int ideaId, CancellationToken ct)
    {
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null || idea.Status != IdeaStatus.Published)
        {
            throw new NotFoundException("Idea not found.");
        }

        return idea;
    }

    private static ReviewDto ToDto(IdeaReview review, int? viewerId) => new()
    {
        Id = review.Id,
        Body = review.Body,
        Rating = review.Rating,
        IsGuest = review.IsGuest,
        ModerationStatus = review.ModerationStatus,
        DisplayName = "Anonymous",
        CreatedAt = review.CreatedAt,
        CanEdit = !review.IsGuest && viewerId != null && review.ReviewerUserId == viewerId
    };
}


