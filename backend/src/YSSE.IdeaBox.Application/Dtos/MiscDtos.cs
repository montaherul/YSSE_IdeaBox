using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record NotificationDto
{
    public int Id { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? LinkUrl { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record NotificationPreferenceDto(NotificationType Type, NotificationChannel Channel, bool IsEnabled);

public record NotificationPreferenceUpdateRequest
{
    [Required] public NotificationType Type { get; init; }
    [Required] public NotificationChannel Channel { get; init; }
    public bool IsEnabled { get; init; }
}

// ---- Projects ----
public record ProjectDto
{
    public int Id { get; init; }
    public int IdeaId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Objectives { get; init; }
    public ProjectStatus Status { get; init; }
    public int LeadUserId { get; init; }
    public IReadOnlyList<ProjectMilestoneDto> Milestones { get; init; } = Array.Empty<ProjectMilestoneDto>();
    public IReadOnlyList<ProjectTaskDto> Tasks { get; init; } = Array.Empty<ProjectTaskDto>();
}

public record ProjectCreateRequest
{
    [Required] public int IdeaId { get; init; }
    [Required, MaxLength(200)] public string Name { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Objectives { get; init; }
}

public record ProjectStatusChangeRequest([Required] ProjectStatus Status);

public record ProjectMilestoneDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime? DueAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record ProjectMilestoneCreateRequest
{
    [Required, MaxLength(200)] public string Title { get; init; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; init; }
    public DateTime? DueAt { get; init; }
}

public record ProjectTaskDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public int? AssigneeUserId { get; init; }
    public DateTime? DueAt { get; init; }
    public bool IsCompleted { get; init; }
}

public record ProjectTaskCreateRequest
{
    [Required, MaxLength(200)] public string Title { get; init; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; init; }
    public int? AssigneeUserId { get; init; }
    public DateTime? DueAt { get; init; }
}

public record ProjectUpdateCreateRequest
{
    [Required, MaxLength(3000)] public string Body { get; init; } = string.Empty;
    [MaxLength(500)] public string? EvidenceUrl { get; init; }
    public bool IsPublic { get; init; }
}

/// <summary>Read model for GET projects/{id}; carries the nested shape the project workspace renders.</summary>
public record ProjectDetailDto
{
    public int Id { get; init; }
    public int IdeaId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Objectives { get; init; }
    public ProjectStatus Status { get; init; }
    public int LeadUserId { get; init; }
    public ProjectIdeaRefDto? Idea { get; init; }
    public IReadOnlyList<ProjectMilestoneDetailDto> Milestones { get; init; } = Array.Empty<ProjectMilestoneDetailDto>();
    public IReadOnlyList<ProjectTaskDetailDto> Tasks { get; init; } = Array.Empty<ProjectTaskDetailDto>();
    public IReadOnlyList<ProjectUpdateDto> Updates { get; init; } = Array.Empty<ProjectUpdateDto>();
}

public record ProjectIdeaRefDto
{
    public int Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
}

public record ProjectMilestoneDetailDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime? DueAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

public record ProjectTaskDetailDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int? AssigneeUserId { get; init; }
    public DateTime? DueAt { get; init; }
    public bool IsDone { get; init; }
}

public record ProjectUpdateDto
{
    public int Id { get; init; }
    public string Body { get; init; } = string.Empty;
    public string? EvidenceUrl { get; init; }
    public bool IsPublic { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ---- Marketplace ----
public record ListingDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Condition { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
    public ListingType ListingType { get; init; }
    public decimal? Price { get; init; }
    public string? CoarseLocation { get; init; }
    public ListingStatus Status { get; init; }
    public int? ActiveReservationId { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ListingCreateRequest
{
    [Required, MaxLength(150)] public string Title { get; init; } = string.Empty;
    [Required, MaxLength(3000)] public string Description { get; init; } = string.Empty;
    [Required, MaxLength(120)] public string Category { get; init; } = string.Empty;
    [Required, MaxLength(60)] public string Condition { get; init; } = string.Empty;
    [MaxLength(500)] public string? PhotoUrl { get; init; }
    [Required] public ListingType ListingType { get; init; }
    public decimal? Price { get; init; }
    [MaxLength(150)] public string? CoarseLocation { get; init; }
}

public record ReservationRequest([MaxLength(1000)] string? Message);

public record ReservationDto
{
    public int Id { get; init; }
    public int MarketplaceListingId { get; init; }
    public string RequesterName { get; init; } = string.Empty;
    public string? Message { get; init; }
    public ReservationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}

// ---- Outcomes ----
public record OutcomeDto
{
    public int Id { get; init; }
    public int IdeaId { get; init; }
    public OutcomeKind Kind { get; init; }
    public OutcomeStatus Status { get; init; }
    public string? Description { get; init; }
    public bool IsVerified { get; init; }
    public bool IsPublic { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record OutcomeCreateRequest
{
    [Required] public int IdeaId { get; init; }
    [Required] public OutcomeKind Kind { get; init; }
    public OutcomeStatus Status { get; init; } = OutcomeStatus.AgreementReported;
    [MaxLength(2000)] public string? Description { get; init; }
}

public record OutcomeVerifyRequest
{
    public bool IsPublic { get; init; }
}

// ---- Special roles ----
public record AssistSubmissionRequest
{
    [Required] public int CreatorUserId { get; init; }
    [Required, MaxLength(50)] public string ConsentMethod { get; init; } = string.Empty;
    [Required] public IdeaCreateRequest Idea { get; init; } = new();
}
