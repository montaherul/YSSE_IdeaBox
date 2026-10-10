using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
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
    public async Task<IActionResult> Create([FromBody] ProblemPostCreateRequest request, CancellationToken ct)
        => OkResult(await _problems.CreateAsync(request, ct));

    [HttpPost("{id:int}/endorse")]
    public async Task<IActionResult> Endorse(int id, CancellationToken ct)
        => OkResult(await _problems.EndorseAsync(id, ct));

    [HttpPost("{id:int}/status")]
    [Authorize(Policy = "Moderator")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ProblemStatusChangeRequest request, CancellationToken ct)
        => OkResult(await _problems.ChangeStatusAsync(id, request, ct));

    [HttpPost("{id:int}/link")]
    public async Task<IActionResult> Link(int id, [FromBody] LinkIdeaToProblemRequest request, CancellationToken ct)
        => OkResult(await _problems.LinkIdeaAsync(id, request, ct));
}
