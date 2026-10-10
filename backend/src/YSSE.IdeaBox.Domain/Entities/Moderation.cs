using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Abuse report / case (FR-G09, FR-I08, FR-K).</summary>
public class Report : BaseEntity
{
    public ReportTargetType TargetType { get; set; }

    public int TargetId { get; set; }

    public int ReporterUserId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Details { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public int? HandledByUserId { get; set; }

    public string? Resolution { get; set; }

    public DateTime? ResolvedAt { get; set; }
}

/// <summary>Append-only administrative/security audit record (FR-K06, NFR-09).</summary>
public class AuditLog : BaseEntity
{
    public int? ActorUserId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityId { get; set; }

    public string? Justification { get; set; }

    public string? Metadata { get; set; }

    /// <summary>Short-lived IP-derived data, hashed.</summary>
    public string? IpHash { get; set; }
}

/// <summary>In-app / email notification (Module J).</summary>
public class Notification : BaseEntity
{
    public int UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    /// <summary>Security/account notices ignore user preferences (Module J).</summary>
    public bool IsEssential { get; set; }
}

public class NotificationPreference : BaseEntity
{
    public int UserId { get; set; }

    public NotificationType Type { get; set; }

    public NotificationChannel Channel { get; set; }

    public bool IsEnabled { get; set; } = true;
}
