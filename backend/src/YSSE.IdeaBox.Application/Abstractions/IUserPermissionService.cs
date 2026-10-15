namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>
/// Resolves the distinct permission codes a user holds through their roles.
/// Used by authorization policies; the database remains the single source of truth.
/// </summary>
public interface IUserPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionsAsync(int userId, CancellationToken ct = default);
}
