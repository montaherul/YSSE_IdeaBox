using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Ideas;

public sealed class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public CommentService(
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

    public async Task<PagedResult<CommentDto>> GetCommentsAsync(int ideaId, int page, int pageSize, CancellationToken ct = default)
    {
        var (p, s) = Pagination.Normalize(page, pageSize);
        var q = _unitOfWork.Repository<IdeaComment>().Query().Where(c => c.IdeaId == ideaId && !c.IsDeleted);
        if (!_current.IsModeratorOrAdmin)
        {
            q = q.Where(c => c.ModerationStatus == ReviewModerationStatus.Published);
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderBy(c => c.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                IdeaId = c.IdeaId,
                ParentCommentId = c.ParentCommentId,
                Body = c.Body,
                AuthorName = c.Idea.Attribution == IdeaAttribution.Anonymous && c.AuthorUserId == c.Idea.CreatorUserId
                    ? "Anonymous"
                    : _unitOfWork.Repository<User>().Query().Where(u => u.Id == c.AuthorUserId).Select(u => u.DisplayName).FirstOrDefault() ?? "Member",
                ModerationStatus = c.ModerationStatus,
                CanDelete = c.AuthorUserId != 0 && (c.AuthorUserId == _current.UserId || _current.IsModeratorOrAdmin),
                CanEdit = c.AuthorUserId != 0 && c.AuthorUserId == _current.UserId,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return PagedResult<CommentDto>.Create(items, p, s, total);
    }

    public async Task<Result<CommentDto>> AddCommentAsync(int ideaId, CommentCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null || idea.Status != IdeaStatus.Published)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (request.ParentCommentId is not null)
        {
            var parentExists = await _unitOfWork.Repository<IdeaComment>().Query().AnyAsync(
                c => c.Id == request.ParentCommentId && c.IdeaId == ideaId && !c.IsDeleted, ct);
            if (!parentExists)
            {
                throw new ConflictException("The parent comment does not exist.");
            }
        }

        var comment = new IdeaComment
        {
            IdeaId = ideaId,
            AuthorUserId = userId,
            ParentCommentId = request.ParentCommentId,
            Body = request.Body,
            ModerationStatus = ReviewModerationStatus.Published,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<IdeaComment>().AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var authorName = await _unitOfWork.Repository<User>().Query().Where(u => u.Id == userId).Select(u => u.DisplayName).FirstOrDefaultAsync(ct) ?? "Anonymous";
        if (idea.CreatorUserId != userId)
        {
            await _notifications.NotifyAsync(idea.CreatorUserId, NotificationType.NewComment,
                "New comment on your idea", $"{authorName} commented on \"{idea.Title}\".", null, ct: ct);
        }

return Result<CommentDto>.Ok(new CommentDto
        {
            Id = comment.Id,
            IdeaId = ideaId,
            ParentCommentId = comment.ParentCommentId,
            Body = comment.Body,
            AuthorName = idea.Attribution == IdeaAttribution.Anonymous && userId == idea.CreatorUserId ? "Anonymous" : authorName,
            ModerationStatus = comment.ModerationStatus,
            CanDelete = true,
            CanEdit = true,
            CreatedAt = comment.CreatedAt
        });
    }

    public async Task<Result> UpdateCommentAsync(int commentId, CommentUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var comment = await _unitOfWork.Repository<IdeaComment>().Query().FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted, ct);
        if (comment is null)
        {
            throw new NotFoundException("Comment not found.");
        }

        if (comment.AuthorUserId != userId)
        {
            throw new ForbiddenException();
        }

        comment.Body = request.Body;
        comment.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("CommentEdited", nameof(IdeaComment), comment.Id.ToString(),
            null, new { comment.IdeaId }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteCommentAsync(int commentId, CancellationToken ct = default)
    {
        var comment = await _unitOfWork.Repository<IdeaComment>().Query().FirstOrDefaultAsync(c => c.Id == commentId && !c.IsDeleted, ct);
        if (comment is null)
        {
            throw new NotFoundException("Comment not found.");
        }

        var isOwner = _current.IsAuthenticated && _current.UserId == comment.AuthorUserId;
        if (!isOwner && !_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        comment.IsDeleted = true;
        comment.DeletedAt = _clock.UtcNow;
        comment.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> ModerateCommentAsync(int commentId, ReviewModerateRequest request, CancellationToken ct = default)
    {
        if (!_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException();
        }

        var comment = await _unitOfWork.Repository<IdeaComment>().Query().FirstOrDefaultAsync(c => c.Id == commentId, ct);
        if (comment is null)
        {
            throw new NotFoundException("Comment not found.");
        }

        comment.ModerationStatus = request.Status;
        comment.ModerationReason = request.Reason;
        comment.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("CommentModerated", nameof(IdeaComment), comment.Id.ToString(),
            request.Reason, new { request.Status }, ct);
        return Result.Ok();
    }

    public async Task<Result> ToggleFollowAsync(int ideaId, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == ideaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        var follow = await _unitOfWork.Repository<IdeaFollow>().Query().FirstOrDefaultAsync(f => f.IdeaId == ideaId && f.UserId == userId, ct);
        if (follow is null)
        {
            await _unitOfWork.Repository<IdeaFollow>().AddAsync(new IdeaFollow { IdeaId = ideaId, UserId = userId, CreatedAt = _clock.UtcNow }, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Ok("Followed.");
        }

        _unitOfWork.Repository<IdeaFollow>().Remove(follow);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok("Unfollowed.");
    }

    public async Task<bool> IsFollowingAsync(int ideaId, CancellationToken ct = default)
    {
        if (!_current.IsAuthenticated || _current.UserId is null)
        {
            return false;
        }

        return await _unitOfWork.Repository<IdeaFollow>().Query().AnyAsync(f => f.IdeaId == ideaId && f.UserId == _current.UserId, ct);
    }

    public async Task<PagedResult<IdeaListItemDto>> GetFollowedAsync(IdeaListQuery query, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);

        var q = _unitOfWork.Repository<Idea>().Query().Where(i => !i.IsDeleted && i.Follows.Any(f => f.UserId == userId));
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(IdeaProjections.List)
            .ToListAsync(ct);

        return PagedResult<IdeaListItemDto>.Create(items, page, pageSize, total);
    }
}


