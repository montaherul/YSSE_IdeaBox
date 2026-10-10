using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class ProjectsController : ApiControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects) => _projects = projects;

    [HttpGet("ideas/{ideaId:int}/projects")]
    [AllowAnonymous]
    public async Task<IActionResult> ForIdea(int ideaId, CancellationToken ct)
        => OkData(await _projects.GetForIdeaAsync(ideaId, ct));

    [HttpGet("projects/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, ct);
        return project is null ? NotFoundError("Project not found.") : OkData(project);
    }

    [HttpPost("projects")]
    public async Task<IActionResult> Create([FromBody] ProjectCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.CreateAsync(request, ct));

    [HttpPost("projects/{id:int}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ProjectStatusChangeRequest request, CancellationToken ct)
        => OkResult(await _projects.ChangeStatusAsync(id, request, ct));

    [HttpPost("projects/{projectId:int}/milestones")]
    public async Task<IActionResult> AddMilestone(int projectId, [FromBody] ProjectMilestoneCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.AddMilestoneAsync(projectId, request, ct));

    [HttpPost("projects/{projectId:int}/tasks")]
    public async Task<IActionResult> AddTask(int projectId, [FromBody] ProjectTaskCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.AddTaskAsync(projectId, request, ct));

    [HttpPost("projects/{projectId:int}/updates")]
    public async Task<IActionResult> PublishUpdate(int projectId, [FromBody] ProjectUpdateCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.PublishUpdateAsync(projectId, request, ct));
}
