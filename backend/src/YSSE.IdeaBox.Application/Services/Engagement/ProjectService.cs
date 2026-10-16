using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

using YSSE.IdeaBox.Application.Services.Common;

namespace YSSE.IdeaBox.Application.Services.Engagement;

public sealed class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;

    public ProjectService(
        IUnitOfWork db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit,
        INotificationDispatcher notifications)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<ProjectDto>> GetForIdeaAsync(int ideaId, CancellationToken ct = default)
    {
        // Projects are internal workspaces: visible to staff and to participants
        // (the idea creator, the selected lead and other project members) only.
        if (!_current.IsModeratorOrAdmin)
        {
            var userId = _current.UserId;
            var allowed = userId is not null &&
                (await IsIdeaCreatorAsync(ideaId, userId.Value, ct) ||
                 await _unitOfWork.Repository<ProjectMember>().Query().AnyAsync(m => m.Project.IdeaId == ideaId && m.UserId == userId, ct));
            if (!allowed)
            {
                throw new ForbiddenException("Only staff and project participants can view projects.");
            }
        }

        return await ProjectQuery(
            _unitOfWork.Repository<Project>().Query().Where(p => p.IdeaId == ideaId && !p.IsDeleted), ct);
    }

    public async Task<IReadOnlyList<ProjectDto>> GetMineAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        // Projects assigned by staff appear on the dashboard of everyone involved:
        // the selected lead, other project members, and the creator of the originating idea.
        return await ProjectQuery(
            _unitOfWork.Repository<Project>().Query().Where(p => !p.IsDeleted &&
                (p.LeadUserId == userId ||
                 p.Idea.CreatorUserId == userId ||
                 p.Members.Any(m => m.UserId == userId))), ct);
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (project is null)
        {
            return null;
        }

        if (!_current.IsModeratorOrAdmin)
        {
            var userId = _current.UserId;
            var allowed = userId is not null &&
                (await IsIdeaCreatorAsync(project.IdeaId, userId.Value, ct) ||
                 await _unitOfWork.Repository<ProjectMember>().Query().AnyAsync(m => m.ProjectId == project.Id && m.UserId == userId, ct));
            if (!allowed)
            {
                throw new ForbiddenException("Only staff and project participants can view this project.");
            }
        }

        return await _unitOfWork.Repository<Project>().Query()
            .Where(p => p.Id == id)
            .Select(p => new ProjectDetailDto
            {
                Id = p.Id,
                IdeaId = p.IdeaId,
                Name = p.Name,
                Objectives = p.Objectives,
                Status = p.Status,
                LeadUserId = p.LeadUserId,
                StartedAt = p.StartedAt,
                CompletedAt = p.CompletedAt,
                ArchivedAt = p.ArchivedAt,
                Idea = new ProjectIdeaRefDto { Id = p.Idea.Id, Slug = p.Idea.Slug, Title = p.Idea.Title },
                Milestones = p.Milestones.Select(m => new ProjectMilestoneDetailDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    DueAt = m.DueAt,
                    CompletedAt = m.CompletedAt
                }).ToList(),
                Tasks = p.Tasks.Select(t => new ProjectTaskDetailDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    AssigneeUserId = t.AssigneeUserId,
                    DueAt = t.DueAt,
                    IsDone = t.IsCompleted
                }).ToList(),
                Updates = p.Updates
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new ProjectUpdateDto
                    {
                        Id = u.Id,
                        Body = u.Body,
                        EvidenceUrl = u.EvidenceUrl,
                        IsPublic = u.IsPublic,
                        CreatedAt = u.CreatedAt
                    }).ToList()
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Result<int>> CreateAsync(ProjectCreateRequest request, CancellationToken ct = default)
    {
        _current.RequireUserId();

        // Projects are assigned by staff: an admin (or moderator) selects the idea and
        // the entrepreneur/investor who will lead the project.
        if (!_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException("Only staff can create a project.");
        }

        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == request.IdeaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        if (!_current.IsAdmin &&
            idea.Status is IdeaStatus.Draft or IdeaStatus.Rejected or IdeaStatus.Withdrawn
                or IdeaStatus.Archived or IdeaStatus.RevisionRequired)
        {
            return Result<int>.Fail("Projects can only be started from a live or approved idea.");
        }

        var lead = await _unitOfWork.Repository<User>().Query()
            .Where(u => u.Id == request.LeadUserId && !u.IsDeleted)
            .Select(u => new { u.IsSuspended, Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList() })
            .FirstOrDefaultAsync(ct);

        if (lead is null)
        {
            return Result<int>.Fail("Select a valid user to lead this project.");
        }

        if (lead.IsSuspended)
        {
            return Result<int>.Fail("The selected user's account is suspended.");
        }

        var isEntrepreneurOrInvestor = lead.Roles.Any(r =>
            string.Equals(r, SystemRoles.Entrepreneur, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, SystemRoles.Investor, StringComparison.OrdinalIgnoreCase));
        if (!isEntrepreneurOrInvestor)
        {
            return Result<int>.Fail("The project lead must be an entrepreneur or an investor.");
        }

        var project = new Project
        {
            IdeaId = idea.Id,
            Name = request.Name.Trim(),
            Objectives = request.Objectives,
            Status = ProjectStatus.Planned,
            LeadUserId = request.LeadUserId,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Project>().AddAsync(project, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _unitOfWork.Repository<ProjectMember>().AddAsync(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = request.LeadUserId,
            Role = "Lead",
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("ProjectCreated", nameof(Project), project.Id.ToString(), null,
            new { project.Name, LeadUserId = request.LeadUserId }, ct);

        await _notifications.NotifyAsync(request.LeadUserId, NotificationType.ProjectAssigned,
            "Project assigned", $"You have been assigned to lead the project \"{project.Name}\".", null, ct: ct);

        return Result<int>.Ok(project.Id);
    }

    public async Task<Result> UpdateAsync(int id, ProjectUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        await RequireMemberAsync(project.Id, userId, ct);

        project.Name = request.Name.Trim();
        project.Objectives = request.Objectives;
        project.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("ProjectUpdated", nameof(Project), project.Id.ToString(), null, new { project.Name }, ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        // Only the project lead or a platform administrator may delete a project.
        if (project.LeadUserId != userId && !_current.IsAdmin)
        {
            throw new ForbiddenException("Only the project lead can delete this project.");
        }

        project.IsDeleted = true;
        project.DeletedAt = _clock.UtcNow;
        project.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("ProjectDeleted", nameof(Project), project.Id.ToString(), null, new { project.Name }, ct);
        return Result.Ok();
    }

    public async Task<Result> ChangeStatusAsync(int id, ProjectStatusChangeRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        if (project.Status == ProjectStatus.Archived)
        {
            return Result.Fail("Archived projects are final and cannot change status.");
        }

        if (request.Status == ProjectStatus.Archived)
        {
            return Result.Fail("Use the archive action to archive a completed project.");
        }

        var isLead = project.LeadUserId == userId;
        var isMember = await _unitOfWork.Repository<ProjectMember>().Query().AnyAsync(m => m.ProjectId == project.Id && m.UserId == userId, ct);
        if (!isLead && !isMember && !_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        project.Status = request.Status;
        if (request.Status == ProjectStatus.Completed)
        {
            project.CompletedAt = _clock.UtcNow;
        }

        if (request.Status == ProjectStatus.InProgress && project.StartedAt is null)
        {
            project.StartedAt = _clock.UtcNow;
        }

        project.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>Admin finalization: a successfully completed project is archived as a success.</summary>
    public async Task<Result> ArchiveAsync(int id, CancellationToken ct = default)
    {
        _current.RequireUserId();
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException("Only an administrator can archive a project.");
        }

        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        if (project.Status == ProjectStatus.Archived)
        {
            return Result.Fail("This project is already archived.");
        }

        if (project.Status != ProjectStatus.Completed)
        {
            return Result.Fail("Only a successfully completed project can be archived.");
        }

        project.Status = ProjectStatus.Archived;
        project.ArchivedAt = _clock.UtcNow;
        project.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("ProjectArchived", nameof(Project), project.Id.ToString(), null, new { project.Name }, ct);
        return Result.Ok();
    }

    public async Task<PagedResult<ProjectArchiveItemDto>> GetArchivedAsync(ProjectArchiveQuery query, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException("Only an administrator can view the project archive.");
        }

        var (page, pageSize) = Pagination.Normalize(query.Page, query.PageSize);
        var q = _unitOfWork.Repository<Project>().Query()
            .Where(p => !p.IsDeleted && p.Status == ProjectStatus.Archived);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim().ToLower()}%";
            q = q.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), pattern) ||
                EF.Functions.Like(p.Idea.Title.ToLower(), pattern));
        }

        var total = await q.CountAsync(ct);
        var users = _unitOfWork.Repository<User>();
        var items = await q
            .OrderByDescending(p => p.ArchivedAt ?? p.UpdatedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectArchiveItemDto
            {
                Id = p.Id,
                Name = p.Name,
                IdeaId = p.IdeaId,
                IdeaTitle = p.Idea.Title,
                IdeaSlug = p.Idea.Slug,
                LeadUserId = p.LeadUserId,
                LeadDisplayName = users.Query()
                    .Where(u => u.Id == p.LeadUserId)
                    .Select(u => u.DisplayName)
                    .FirstOrDefault() ?? "Member",
                CompletedAt = p.CompletedAt,
                ArchivedAt = p.ArchivedAt
            })
            .ToListAsync(ct);

        return PagedResult<ProjectArchiveItemDto>.Create(items, page, pageSize, total);
    }

    /// <summary>Users eligible to lead a project (must hold the Entrepreneur or Investor role).</summary>
    public async Task<IReadOnlyList<ProjectLeadCandidateDto>> GetLeadCandidatesAsync(string? search, CancellationToken ct = default)
    {
        if (!_current.IsModeratorOrAdmin)
        {
            throw new ForbiddenException("Only staff can select a project lead.");
        }

        var q = _unitOfWork.Repository<User>().Query()
            .Where(u => !u.IsDeleted && !u.IsSuspended &&
                        u.UserRoles.Any(ur =>
                            ur.Role.Name == SystemRoles.Entrepreneur ||
                            ur.Role.Name == SystemRoles.Investor));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().ToLower()}%";
            q = q.Where(u => EF.Functions.Like(u.DisplayName.ToLower(), pattern));
        }

        return await q
            .OrderBy(u => u.DisplayName)
            .Take(20)
            .Select(u => new ProjectLeadCandidateDto
            {
                UserId = u.Id,
                DisplayName = u.DisplayName,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<Result<int>> AddMilestoneAsync(int projectId, ProjectMilestoneCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        await RequireMemberAsync(project.Id, userId, ct);

        var milestone = new ProjectMilestone
        {
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description,
            DueAt = request.DueAt,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<ProjectMilestone>().AddAsync(milestone, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(milestone.Id);
    }

    public async Task<Result<int>> AddTaskAsync(int projectId, ProjectTaskCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        await RequireMemberAsync(project.Id, userId, ct);

        var task = new ProjectTask
        {
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description,
            AssigneeUserId = request.AssigneeUserId,
            DueAt = request.DueAt,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<ProjectTask>().AddAsync(task, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(task.Id);
    }

    public async Task<Result> PublishUpdateAsync(int projectId, ProjectUpdateCreateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
        }

        await RequireMemberAsync(project.Id, userId, ct);

        if (request.IsPublic && !_current.IsAdmin && project.LeadUserId != userId)
        {
            throw new ForbiddenException("Only the project lead can publish a public update.");
        }

        await _unitOfWork.Repository<ProjectUpdate>().AddAsync(new ProjectUpdate
        {
            ProjectId = projectId,
            AuthorUserId = userId,
            Body = request.Body,
            EvidenceUrl = request.EvidenceUrl,
            IsPublic = request.IsPublic,
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(project.LeadUserId, NotificationType.OutcomeUpdate,
            "Project update", $"A project update was posted for \"{project.Name}\".", null, ct: ct);

        return Result.Ok();
    }

    private async Task<IReadOnlyList<ProjectDto>> ProjectQuery(IQueryable<Project> q, CancellationToken ct) =>
        await q
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                IdeaId = p.IdeaId,
                Name = p.Name,
                Objectives = p.Objectives,
                Status = p.Status,
                LeadUserId = p.LeadUserId,
                Milestones = p.Milestones.Select(m => new ProjectMilestoneDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    DueAt = m.DueAt,
                    CompletedAt = m.CompletedAt
                }).ToList(),
                Tasks = p.Tasks.Select(t => new ProjectTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    AssigneeUserId = t.AssigneeUserId,
                    DueAt = t.DueAt,
                    IsCompleted = t.IsCompleted
                }).ToList()
            })
            .ToListAsync(ct);

    private async Task<bool> IsIdeaCreatorAsync(int ideaId, int userId, CancellationToken ct) =>
        await _unitOfWork.Repository<Idea>().Query()
            .AnyAsync(i => i.Id == ideaId && i.CreatorUserId == userId && !i.IsDeleted, ct);

    private async Task RequireMemberAsync(int projectId, int userId, CancellationToken ct)
    {
        var lead = await _unitOfWork.Repository<Project>().Query().AnyAsync(p => p.Id == projectId && p.LeadUserId == userId, ct);
        var member = await _unitOfWork.Repository<ProjectMember>().Query().AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);
        if (!lead && !member && !_current.IsAdmin)
        {
            throw new ForbiddenException();
        }
    }
}


