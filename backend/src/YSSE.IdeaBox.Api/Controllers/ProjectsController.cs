using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
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

    [HttpGet("projects/archive")]
    [Authorize(Policy = Permissions.ProjectsManage)]
    public async Task<IActionResult> Archive([FromQuery] ProjectArchiveQuery query, CancellationToken ct)
        => Paged(await _projects.GetArchivedAsync(query, ct));

    [HttpGet("projects/mine")]
    public async Task<IActionResult> Mine(CancellationToken ct)
        => OkData(await _projects.GetMineAsync(ct));

    [HttpGet("projects/lead-candidates")]
    [Authorize(Policy = Permissions.ProjectsCreate)]
    public async Task<IActionResult> LeadCandidates([FromQuery] string? search, CancellationToken ct)
        => OkData(await _projects.GetLeadCandidatesAsync(search, ct));

    [HttpGet("projects/{id:int}")]
    [Authorize(Policy = Permissions.ProjectsRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, ct);
        return project is null ? NotFoundError("Project not found.") : OkData(project);
    }

    [HttpPost("projects")]
    [Authorize(Policy = Permissions.ProjectsCreate)]
    public async Task<IActionResult> Create([FromBody] ProjectCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.CreateAsync(request, ct));

    [HttpPost("projects/{id:int}/archive")]
    [Authorize(Policy = Permissions.ProjectsManage)]
    public async Task<IActionResult> ArchiveProject(int id, CancellationToken ct)
        => OkResult(await _projects.ArchiveAsync(id, ct));

    [HttpPut("projects/{id:int}")]
    [Authorize(Policy = Permissions.ProjectsUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] ProjectUpdateRequest request, CancellationToken ct)
        => OkResult(await _projects.UpdateAsync(id, request, ct));

    [HttpDelete("projects/{id:int}")]
    [Authorize(Policy = Permissions.ProjectsDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _projects.DeleteAsync(id, ct));

    [HttpPost("projects/{id:int}/status")]
    [Authorize(Policy = Permissions.ProjectsUpdate)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ProjectStatusChangeRequest request, CancellationToken ct)
        => OkResult(await _projects.ChangeStatusAsync(id, request, ct));

    [HttpPost("projects/{projectId:int}/milestones")]
    [Authorize(Policy = Permissions.ProjectsUpdate)]
    public async Task<IActionResult> AddMilestone(int projectId, [FromBody] ProjectMilestoneCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.AddMilestoneAsync(projectId, request, ct));

    [HttpPost("projects/{projectId:int}/tasks")]
    [Authorize(Policy = Permissions.ProjectsUpdate)]
    public async Task<IActionResult> AddTask(int projectId, [FromBody] ProjectTaskCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.AddTaskAsync(projectId, request, ct));

    [HttpPost("projects/{projectId:int}/updates")]
    [Authorize(Policy = Permissions.ProjectsUpdate)]
    public async Task<IActionResult> PublishUpdate(int projectId, [FromBody] ProjectUpdateCreateRequest request, CancellationToken ct)
        => OkResult(await _projects.PublishUpdateAsync(projectId, request, ct));
}