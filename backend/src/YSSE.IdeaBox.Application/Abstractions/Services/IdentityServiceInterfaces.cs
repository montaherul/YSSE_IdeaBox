using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Application.Abstractions.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default);

    Task<Result> LogoutAsync(RefreshRequest request, CancellationToken ct = default);

    Task<UserDto?> GetCurrentUserAsync(CancellationToken ct = default);

    Task<Result> RequestVerificationAsync(CancellationToken ct = default);

    Task<Result> VerifyAsync(VerifyRequest request, CancellationToken ct = default);

    Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);

    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
}

public interface IProfileService
{
    Task<MemberProfileDto?> GetMemberProfileAsync(int userId, CancellationToken ct = default);

    Task<Result> UpdateMemberProfileAsync(MemberProfileUpdateRequest request, CancellationToken ct = default);

    Task<CreatorProfileDto?> GetMyCreatorProfileAsync(CancellationToken ct = default);

    Task<CreatorProfileDto?> GetPublicCreatorProfileAsync(int userId, CancellationToken ct = default);

    Task<Result<int>> UpsertCreatorProfileAsync(CreatorProfileUpdateRequest request, CancellationToken ct = default);

    Task<Result> DeleteCreatorProfileAsync(CancellationToken ct = default);

    Task<EntrepreneurProfileDto?> GetMyEntrepreneurProfileAsync(CancellationToken ct = default);

    Task<Result<int>> RequestOpportunitySeekerAsync(EntrepreneurProfileUpdateRequest request, CancellationToken ct = default);

    Task<PagedResult<EntrepreneurProfileDto>> GetPendingSeekersAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Result> ApproveSeekerAsync(int userId, bool approve, CancellationToken ct = default);
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(bool includeInactive, CancellationToken ct = default);

    Task<IReadOnlyList<LookupDto>> GetTagsAsync(CancellationToken ct = default);

    Task<Result<int>> CreateCategoryAsync(CategoryUpsertRequest request, CancellationToken ct = default);

    Task<Result> UpdateCategoryAsync(int id, CategoryUpsertRequest request, CancellationToken ct = default);

    Task<Result> DeleteCategoryAsync(int id, CancellationToken ct = default);
}
