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
        var userId = _current.UserId;
        if (_current.IsAdmin)
        {
            return await ProjectQuery(_unitOfWork.Repository<Project>().Query().Where(p => p.IdeaId == ideaId && !p.IsDeleted), ct);
        }

        var authorized = userId is not null && await IsParticipantAsync(ideaId, userId.Value, ct);
        var q = _unitOfWork.Repository<Project>().Query().Where(p => p.IdeaId == ideaId && !p.IsDeleted);
        if (!authorized)
        {
            if (!await IsPublicIdeaAsync(ideaId, ct))
            {
                throw new ForbiddenException();
            }
        }

        return await ProjectQuery(q, ct);
    }

    public async Task<ProjectDetailDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (project is null)
        {
            return null;
        }

        if (!_current.IsAdmin)
        {
            var userId = _current.UserId;
            var authorized = userId is not null && await IsParticipantAsync(project.IdeaId, userId.Value, ct);
            if (!authorized && !await IsPublicIdeaAsync(project.IdeaId, ct))
            {
                throw new ForbiddenException();
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
        var userId = _current.RequireUserId();
        var idea = await _unitOfWork.Repository<Idea>().Query().FirstOrDefaultAsync(i => i.Id == request.IdeaId && !i.IsDeleted, ct);
        if (idea is null)
        {
            throw new NotFoundException("Idea not found.");
        }

        var isCreator = idea.CreatorUserId == userId;
        var isSeeker = await _unitOfWork.Repository<EntrepreneurInquiry>().Query().AnyAsync(i => i.IdeaId == idea.Id && i.SeekerUserId == userId, ct);
        if (!isCreator && !isSeeker && !_current.IsAdmin)
        {
            throw new ForbiddenException("Only participants can create a project for this idea.");
        }

        var project = new Project
        {
            IdeaId = idea.Id,
            Name = request.Name.Trim(),
            Objectives = request.Objectives,
            Status = ProjectStatus.Planned,
            LeadUserId = userId,
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<Project>().AddAsync(project, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _unitOfWork.Repository<ProjectMember>().AddAsync(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = "Lead",
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<int>.Ok(project.Id);
    }

    public async Task<Result> ChangeStatusAsync(int id, ProjectStatusChangeRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var project = await _unitOfWork.Repository<Project>().Query().FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);
        if (project is null)
        {
            throw new NotFoundException("Project not found.");
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

    private async Task<bool> IsPublicIdeaAsync(int ideaId, CancellationToken ct) =>
        await _unitOfWork.Repository<Idea>().Query().AnyAsync(i => i.Id == ideaId && !i.IsDeleted && i.Status == IdeaStatus.Published, ct);

    private async Task<bool> IsParticipantAsync(int ideaId, int userId, CancellationToken ct)
    {
        if (await _unitOfWork.Repository<Idea>().Query().AnyAsync(i => i.Id == ideaId && i.CreatorUserId == userId && !i.IsDeleted, ct))
        {
            return true;
        }

        return await _unitOfWork.Repository<EntrepreneurInquiry>().Query().AnyAsync(i => i.IdeaId == ideaId && i.SeekerUserId == userId, ct) ||
               await _unitOfWork.Repository<ProjectMember>().Query().AnyAsync(m => m.Project.IdeaId == ideaId && m.UserId == userId, ct);
    }

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


