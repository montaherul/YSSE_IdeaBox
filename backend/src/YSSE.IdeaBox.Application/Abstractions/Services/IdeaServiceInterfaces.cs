using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Abstractions.Services;

public interface IIdeaService
{
    Task<PagedResult<IdeaListItemDto>> SearchAsync(IdeaListQuery query, CancellationToken ct = default);

    Task<IdeaDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<IdeaDetailDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Result<int>> CreateAsync(IdeaCreateRequest request, CancellationToken ct = default);

    Task<Result<int>> CreateQuickAsync(QuickIdeaRequest request, CancellationToken ct = default);

    Task<Result> UpdateAsync(int id, IdeaUpdateRequest request, CancellationToken ct = default);

    Task<Result> SubmitAsync(int id, CancellationToken ct = default);

    Task<Result> WithdrawAsync(int id, CancellationToken ct = default);

    Task<Result> DeleteDraftAsync(int id, CancellationToken ct = default);

    Task<PagedResult<IdeaListItemDto>> GetMyIdeasAsync(IdeaListQuery query, CancellationToken ct = default);

    Task<IReadOnlyList<IdeaListItemDto>> GetSimilarAsync(int id, int take = 5, CancellationToken ct = default);

    Task RecordViewAsync(int id, CancellationToken ct = default);
}

public interface IScreeningService
{
    Task<PagedResult<IdeaListItemDto>> GetQueueAsync(IdeaListQuery query, CancellationToken ct = default);

    Task<Result> DecideAsync(int ideaId, ScreeningDecisionRequest request, CancellationToken ct = default);

    Task<Result> ChangeStatusAsync(int ideaId, StatusChangeRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<IdeaScreeningDecisionDto>> GetDecisionsAsync(int ideaId, CancellationToken ct = default);
}

public interface ICommentService
{
    Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int ideaId, CancellationToken ct = default);

    Task<Result<CommentDto>> AddCommentAsync(int ideaId, CommentCreateRequest request, CancellationToken ct = default);

    Task<Result> DeleteCommentAsync(int commentId, CancellationToken ct = default);

    Task<Result> ModerateCommentAsync(int commentId, ReviewModerateRequest request, CancellationToken ct = default);

    Task<Result> ToggleFollowAsync(int ideaId, CancellationToken ct = default);

    Task<bool> IsFollowingAsync(int ideaId, CancellationToken ct = default);

    Task<PagedResult<IdeaListItemDto>> GetFollowedAsync(IdeaListQuery query, CancellationToken ct = default);
}

public interface IProblemService
{
    Task<PagedResult<ProblemPostDto>> ListAsync(ProblemListQuery query, CancellationToken ct = default);

    Task<ProblemPostDto?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Result<int>> CreateAsync(ProblemPostCreateRequest request, CancellationToken ct = default);

    Task<Result> EndorseAsync(int id, CancellationToken ct = default);

    Task<Result> ChangeStatusAsync(int id, ProblemStatusChangeRequest request, CancellationToken ct = default);

    Task<Result> LinkIdeaAsync(int id, LinkIdeaToProblemRequest request, CancellationToken ct = default);
}
