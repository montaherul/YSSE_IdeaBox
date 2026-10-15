using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/problems")]
public sealed class ProblemsController : ApiControllerBase
{
    private readonly IProblemService _problems;

    public ProblemsController(IProblemService problems) => _problems = problems;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] ProblemListQuery query, CancellationToken ct)
        => Paged(await _problems.ListAsync(query, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var problem = await _problems.GetByIdAsync(id, ct);
        return problem is null ? NotFoundError("Problem not found.") : OkData(problem);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.ProblemsCreate)]
    public async Task<IActionResult> Create([FromBody] ProblemPostCreateRequest request, CancellationToken ct)
        => OkResult(await _problems.CreateAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.ProblemsUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] ProblemPostCreateRequest request, CancellationToken ct)
        => OkResult(await _problems.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.ProblemsUpdate)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _problems.DeleteAsync(id, ct));

    [HttpPost("{id:int}/endorse")]
    [Authorize(Policy = Permissions.ProblemsRead)]
    public async Task<IActionResult> Endorse(int id, CancellationToken ct)
        => OkResult(await _problems.EndorseAsync(id, ct));

    [HttpPost("{id:int}/status")]
    [Authorize(Policy = Permissions.ProblemsModerate)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ProblemStatusChangeRequest request, CancellationToken ct)
        => OkResult(await _problems.ChangeStatusAsync(id, request, ct));

    [HttpPost("{id:int}/link")]
    [Authorize(Policy = Permissions.ProblemsUpdate)]
    public async Task<IActionResult> Link(int id, [FromBody] LinkIdeaToProblemRequest request, CancellationToken ct)
        => OkResult(await _problems.LinkIdeaAsync(id, request, ct));
}