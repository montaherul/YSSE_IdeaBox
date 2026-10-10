using YSSE.IdeaBox.Domain.Common;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Domain.Entities;

/// <summary>Optional project workspace linked to an idea (Module L).</summary>
public class Project : BaseEntity, ISoftDeletable
{
    public int IdeaId { get; set; }

    public Idea Idea { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string? Objectives { get; set; }

    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

    public int LeadUserId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

    public ICollection<ProjectUpdate> Updates { get; set; } = new List<ProjectUpdate>();
}

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int UserId { get; set; }

    public string Role { get; set; } = "Member";
}

public class ProjectMilestone : BaseEntity
{
    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}

public class ProjectTask : BaseEntity
{
    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? AssigneeUserId { get; set; }

    public DateTime? DueAt { get; set; }

    public bool IsCompleted { get; set; }
}

/// <summary>Progress update; public only with participant authorization (FR-L08).</summary>
public class ProjectUpdate : BaseEntity
{
    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int AuthorUserId { get; set; }

    public string Body { get; set; } = string.Empty;

    public string? EvidenceUrl { get; set; }

    public bool IsPublic { get; set; }
}
