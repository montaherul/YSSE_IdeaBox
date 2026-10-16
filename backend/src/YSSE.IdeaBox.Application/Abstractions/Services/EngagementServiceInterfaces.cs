using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Abstractions.Services;

public interface IReviewService
{
    Task<ReviewCountsDto> GetCountsAsync(int ideaId, CancellationToken ct = default);

    Task<PagedResult<ReviewDto>> GetForIdeaAsync(int ideaId, int page, int pageSize, CancellationToken ct = default);

    Task<PagedResult<MyReviewDto>> GetMineAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Result<ReviewDto>> SubmitRegisteredAsync(int ideaId, ReviewCreateRequest request, CancellationToken ct = default);

    Task<Result<ReviewDto>> SubmitGuestAsync(int ideaId, ReviewCreateRequest request, GuestContext guest, CancellationToken ct = default);

    Task<Result<ReviewDto>> EditOwnAsync(int reviewId, ReviewCreateRequest request, CancellationToken ct = default);

    Task<Result> DeleteOwnAsync(int reviewId, CancellationToken ct = default);

    Task<Result> ModerateAsync(int reviewId, ReviewModerateRequest request, CancellationToken ct = default);
}

public interface IVotingService
{
    Task<VoteResultDto> GetVoteStateAsync(int ideaId, CancellationToken ct = default);

    Task<Result<VoteResultDto>> CastAsync(int ideaId, CancellationToken ct = default);

    Task<Result<VoteResultDto>> RemoveAsync(int ideaId, CancellationToken ct = default);

    Task<IReadOnlyList<VotingPeriodDto>> GetPeriodsAsync(CancellationToken ct = default);

    Task<Result<int>> CreatePeriodAsync(VotingPeriodCreateRequest request, CancellationToken ct = default);

    Task<Result> UpdatePeriodAsync(int id, VotingPeriodCreateRequest request, CancellationToken ct = default);

    Task<Result> DeletePeriodAsync(int id, CancellationToken ct = default);

    Task<Result> SetPeriodActiveAsync(int id, bool active, CancellationToken ct = default);

    Task<Result> ClosePeriodAsync(int id, CancellationToken ct = default);
}

public interface ILeaderboardService
{
    Task<IReadOnlyList<LeaderboardIdeaDto>> GetTopIdeasAsync(int? votingPeriodId, int take = 3, CancellationToken ct = default);

    Task<IReadOnlyList<IdeaMakerLeaderboardDto>> GetTopIdeaMakersAsync(int? votingPeriodId, int take = 3, CancellationToken ct = default);

    Task<MonthlyTopDto> GetMonthlyTopIdeasAsync(int take = 3, CancellationToken ct = default);

    Task<RecognitionResultDto> AnnounceMonthlyTopAsync(string? message = null, CancellationToken ct = default);
}

public interface IInquiryService
{
    Task<Result<int>> SendAsync(int ideaId, InquiryCreateRequest request, CancellationToken ct = default);

    Task<PagedResult<InquiryDto>> GetReceivedAsync(int page, int pageSize, CancellationToken ct = default);

    Task<PagedResult<InquiryDto>> GetSentAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Result<ConversationRef>> RespondAsync(int inquiryId, InquiryRespondRequest request, CancellationToken ct = default);

    Task<Result> UpdateAsync(int inquiryId, InquiryUpdateRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(int inquiryId, CancellationToken ct = default);

    Task<PagedResult<IdeaListItemDto>> GetWatchlistAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Result> AddWatchlistAsync(int ideaId, CancellationToken ct = default);

    Task<Result> RemoveWatchlistAsync(int ideaId, CancellationToken ct = default);

    Task<IReadOnlyList<MatchResultDto>> GetMatchesAsync(int take = 10, CancellationToken ct = default);
}

public interface IMessagingService
{
    Task<PagedResult<ConversationListItemDto>> GetConversationsAsync(int page, int pageSize, CancellationToken ct = default);

    Task<ConversationDetailDto?> GetConversationAsync(int id, int page, int pageSize, CancellationToken ct = default);

    Task<Result<int>> StartAsync(StartConversationRequest request, CancellationToken ct = default);

    Task<Result<MessageDto>> SendMessageAsync(int conversationId, MessageSendRequest request, CancellationToken ct = default);

    Task<Result> UpdateMessageAsync(int conversationId, int messageId, MessageUpdateRequest request, CancellationToken ct = default);

    Task<Result> DeleteMessageAsync(int conversationId, int messageId, CancellationToken ct = default);

    Task<Result> MarkReadAsync(int conversationId, CancellationToken ct = default);

    Task<Result> SetStatusAsync(int conversationId, ConversationStatus status, CancellationToken ct = default);

    Task<Result> BlockUserAsync(int userId, CancellationToken ct = default);
}

public interface IReportService
{
    Task<Result> CreateAsync(ReportCreateRequest request, CancellationToken ct = default);

    Task<PagedResult<ReportDto>> ListAsync(ReportStatus? status, int page, int pageSize, CancellationToken ct = default);

    Task<Result> UpdateAsync(int id, ReportUpdateRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<Result> ResolveAsync(int id, ReportResolveRequest request, CancellationToken ct = default);
}

public interface IOutcomeService
{
    Task<PagedResult<OutcomeDto>> GetForIdeaAsync(int ideaId, int page, int pageSize, CancellationToken ct = default);

    Task<Result<int>> CreateAsync(OutcomeCreateRequest request, CancellationToken ct = default);

    Task<Result> UpdateAsync(int id, OutcomeUpdateRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<Result> VerifyAsync(int id, OutcomeVerifyRequest request, CancellationToken ct = default);
}

public interface IProjectService
{
    Task<IReadOnlyList<ProjectDto>> GetForIdeaAsync(int ideaId, CancellationToken ct = default);

    /// <summary>Projects assigned to the current user: as the selected lead, a project member, or the idea's creator.</summary>
    Task<IReadOnlyList<ProjectDto>> GetMineAsync(CancellationToken ct = default);

    Task<ProjectDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<PagedResult<ProjectArchiveItemDto>> GetArchivedAsync(ProjectArchiveQuery query, CancellationToken ct = default);

    Task<IReadOnlyList<ProjectLeadCandidateDto>> GetLeadCandidatesAsync(string? search, CancellationToken ct = default);

    Task<Result<int>> CreateAsync(ProjectCreateRequest request, CancellationToken ct = default);

    Task<Result> UpdateAsync(int id, ProjectUpdateRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<Result> ChangeStatusAsync(int id, ProjectStatusChangeRequest request, CancellationToken ct = default);

    Task<Result> ArchiveAsync(int id, CancellationToken ct = default);

    Task<Result<int>> AddMilestoneAsync(int projectId, ProjectMilestoneCreateRequest request, CancellationToken ct = default);

    Task<Result<int>> AddTaskAsync(int projectId, ProjectTaskCreateRequest request, CancellationToken ct = default);

    Task<Result> PublishUpdateAsync(int projectId, ProjectUpdateCreateRequest request, CancellationToken ct = default);
}

public interface IMarketplaceService
{
    Task<PagedResult<ListingDto>> SearchAsync(ListingQuery query, CancellationToken ct = default);

    Task<ListingDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Result<int>> CreateAsync(ListingCreateRequest request, CancellationToken ct = default);

    Task<Result> UpdateAsync(int id, ListingCreateRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<Result> UpdateStatusAsync(int id, ListingStatus status, CancellationToken ct = default);

    Task<Result<int>> RequestReservationAsync(int listingId, ReservationRequest request, CancellationToken ct = default);

    Task<Result> RespondReservationAsync(int reservationId, bool accept, CancellationToken ct = default);

    Task<PagedResult<ListingDto>> GetMyListingsAsync(int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<ReservationDto>> GetReservationsForListingAsync(int listingId, CancellationToken ct = default);
}

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetMineAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Result> MarkReadAsync(int id, CancellationToken ct = default);

    Task<Result> MarkAllReadAsync(CancellationToken ct = default);

    Task<Result> DeleteAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<NotificationPreferenceDto>> GetPreferencesAsync(CancellationToken ct = default);

    Task<Result> UpdatePreferenceAsync(NotificationPreferenceUpdateRequest request, CancellationToken ct = default);
}
