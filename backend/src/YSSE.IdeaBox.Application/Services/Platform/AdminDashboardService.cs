using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Platform;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private static readonly IdeaStatus[] PendingStatuses =
        { IdeaStatus.Submitted, IdeaStatus.UnderReview, IdeaStatus.RevisionRequired };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminDashboardQueries _queries;

    public AdminDashboardService(IUnitOfWork unitOfWork, IAdminDashboardQueries queries)
    {
        _unitOfWork = unitOfWork;
        _queries = queries;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        // EF Core is not thread-safe: run counts sequentially on the shared scoped context.
        var totalIdeas = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted, ct);
        var published = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && i.Status == IdeaStatus.Published, ct);
        var pending = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && PendingStatuses.Contains(i.Status), ct);
        var rejected = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && i.Status == IdeaStatus.Rejected, ct);
        var shortlisted = await _unitOfWork.Repository<Idea>().Query().CountAsync(i => !i.IsDeleted && i.Status == IdeaStatus.Shortlisted, ct);
        var users = await _unitOfWork.Repository<User>().Query().CountAsync(u => !u.IsDeleted, ct);
        var entrepreneurs = await _unitOfWork.Repository<EntrepreneurProfile>().Query().CountAsync(ct);
        var anonymousReviews = await _unitOfWork.Repository<IdeaReview>().Query().CountAsync(r => !r.IsDeleted && r.ReviewerUserId == null, ct);
        var registeredReviews = await _unitOfWork.Repository<IdeaReview>().Query().CountAsync(r => !r.IsDeleted && r.ReviewerUserId != null, ct);
        var votes = await _unitOfWork.Repository<IdeaVote>().Query().CountAsync(v => !v.IsDeleted, ct);
        var inquiries = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().CountAsync(ct);
        var openReports = await _unitOfWork.Repository<Report>().Query().CountAsync(r => r.Status == ReportStatus.Open, ct);
        var listings = await _unitOfWork.Repository<MarketplaceListing>().Query().CountAsync(l => !l.IsDeleted, ct);

        var ideasPerMonth = await _queries.GetIdeasPerMonthAsync(ct);

        return new AdminDashboardDto
        {
            TotalIdeas = totalIdeas,
            PublishedIdeas = published,
            PendingIdeas = pending,
            RejectedIdeas = rejected,
            ShortlistedIdeas = shortlisted,
            TotalUsers = users,
            TotalEntrepreneurs = entrepreneurs,
            AnonymousReviewCount = anonymousReviews,
            RegisteredReviewCount = registeredReviews,
            TotalVotes = votes,
            TotalInquiries = inquiries,
            OpenReports = openReports,
            MarketplaceListings = listings,
            IdeasPerMonth = ideasPerMonth
        };
    }
}
