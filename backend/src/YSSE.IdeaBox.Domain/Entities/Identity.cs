using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>
/// Platform account. A user may hold multiple roles (FR-B: profiles/RBAC).
/// Real name is never required to participate (FR-B25) and anonymous ideas still
/// always record the real creator internally (FR-C10).
/// </summary>
public class User : BaseEntity, ISoftDeletable
{
    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public bool IsPhoneVerified { get; set; }

    public bool IsSuspended { get; set; }

    public string? SuspensionReason { get; set; }

    public bool IsPendingDeletion { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public MemberProfile? MemberProfile { get; set; }

    public CreatorProfile? CreatorProfile { get; set; }

    public EntrepreneurProfile? EntrepreneurProfile { get; set; }
}

/// <summary>RBAC role. Built-in roles are seeded; additional custom roles may be created by admins.</summary>
public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

/// <summary>Many-to-many assignment between users and roles.</summary>
public class UserRole : BaseEntity
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public int? GrantedByUserId { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Fine-grained capability code for administrative configuration.</summary>
public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    public int PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;
}

/// <summary>
/// Persisted refresh token enabling revocation on logout (security rule: expiration + revocation).
/// Stores a hash, never the raw token.
/// </summary>
public class RefreshToken : BaseEntity
{
    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByTokenHash { get; set; }
}

/// <summary>Platform configuration values editable by administrators ([Config] items).</summary>
public class PlatformSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }
}
