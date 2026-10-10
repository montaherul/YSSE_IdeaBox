using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class OutcomesController : ApiControllerBase
{
    private readonly IOutcomeService _outcomes;

    public OutcomesController(IOutcomeService outcomes) => _outcomes = outcomes;

    [HttpGet("ideas/{ideaId:int}/outcomes")]
    [AllowAnonymous]
    public async Task<IActionResult> ForIdea(int ideaId, CancellationToken ct)
        => OkData(await _outcomes.GetForIdeaAsync(ideaId, ct));

    [HttpPost("outcomes")]
    public async Task<IActionResult> Create([FromBody] OutcomeCreateRequest request, CancellationToken ct)
        => OkResult(await _outcomes.CreateAsync(request, ct));

    [HttpPost("outcomes/{id:int}/verify")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Verify(int id, [FromBody] OutcomeVerifyRequest request, CancellationToken ct)
        => OkResult(await _outcomes.VerifyAsync(id, request, ct));
}
