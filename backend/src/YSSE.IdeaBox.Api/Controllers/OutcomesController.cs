using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class OutcomesController : ApiControllerBase
{
    private readonly IOutcomeService _outcomes;

    public OutcomesController(IOutcomeService outcomes) => _outcomes = outcomes;

    [HttpGet("ideas/{ideaId:int}/outcomes")]
    [AllowAnonymous]
    public async Task<IActionResult> ForIdea(int ideaId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _outcomes.GetForIdeaAsync(ideaId, page, pageSize, ct));

    [HttpPost("outcomes")]
    [Authorize(Policy = Permissions.OutcomesCreate)]
    public async Task<IActionResult> Create([FromBody] OutcomeCreateRequest request, CancellationToken ct)
        => OkResult(await _outcomes.CreateAsync(request, ct));

    [HttpPost("outcomes/{id:int}/verify")]
    [Authorize(Policy = Permissions.OutcomesVerify)]
    public async Task<IActionResult> Verify(int id, [FromBody] OutcomeVerifyRequest request, CancellationToken ct)
        => OkResult(await _outcomes.VerifyAsync(id, request, ct));

    [HttpPut("outcomes/{id:int}")]
    [Authorize(Policy = Permissions.OutcomesUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] OutcomeUpdateRequest request, CancellationToken ct)
        => OkResult(await _outcomes.UpdateAsync(id, request, ct));

    [HttpDelete("outcomes/{id:int}")]
    [Authorize(Policy = Permissions.OutcomesDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _outcomes.DeleteAsync(id, ct));
}