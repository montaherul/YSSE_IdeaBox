using System.ComponentModel.DataAnnotations;

namespace YSSE.IdeaBox.Application.Dtos;

public record AdminUserDto
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsSuspended { get; init; }
    public string? SuspensionReason { get; init; }
    public bool IsEmailVerified { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
    public DateTime CreatedAt { get; init; }
}

public record SuspendUserRequest
{
    [Required, MaxLength(500)]
    public string Reason { get; init; } = string.Empty;
}

public record UserCreateRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(120)]
    public string DisplayName { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; init; } = string.Empty;

    [MaxLength(32)]
    public string? PhoneNumber { get; init; }

    /// <summary>Optional additional roles to grant (base Member is always granted).</summary>
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}

public record UserUpdateRequest
{
    [Required, MaxLength(120)]
    public string DisplayName { get; init; } = string.Empty;

    [EmailAddress, MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(32)]
    public string? PhoneNumber { get; init; }

    public bool? IsEmailVerified { get; init; }
}

public record AssignRoleRequest
{
    [Required]
    public string Role { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Justification { get; init; }
}

public record AuditLogDto
{
    public int Id { get; init; }
    public int? ActorUserId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string EntityName { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string? Justification { get; init; }
    public string? Metadata { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PlatformSettingDto(string Key, string Value, string? Description);

public record DashboardMetricDto(string Label, string Value);

public record AdminDashboardDto
{
    public int TotalIdeas { get; init; }
    public int PublishedIdeas { get; init; }
    public int PendingIdeas { get; init; }
    public int RejectedIdeas { get; init; }
    public int ShortlistedIdeas { get; init; }
    public int TotalUsers { get; init; }
    public int TotalEntrepreneurs { get; init; }
    public int AnonymousReviewCount { get; init; }
    public int RegisteredReviewCount { get; init; }
    public int TotalVotes { get; init; }
    public int TotalInquiries { get; init; }
    public int OpenReports { get; init; }
    public int MarketplaceListings { get; init; }
    public IReadOnlyList<DashboardMetricDto> IdeasPerMonth { get; init; } = Array.Empty<DashboardMetricDto>();
}

public record RoleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<PermissionDto> Permissions { get; init; } = Array.Empty<PermissionDto>();
    public int UserCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record RoleUpsertRequest
{
    [Required, MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    /// <summary>Full set of permission ids to grant the role; replaces the current set.</summary>
    public IReadOnlyList<int> PermissionIds { get; init; } = Array.Empty<int>();
}

public record PermissionDto
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int RoleCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PermissionUpsertRequest
{
    [Required, MaxLength(100)]
    public string Code { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}
