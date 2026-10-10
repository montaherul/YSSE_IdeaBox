using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/ideas")]
public sealed class IdeasController : ApiControllerBase
{
    private readonly IIdeaService _ideas;

    public IdeasController(IIdeaService ideas) => _ideas = ideas;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] IdeaListQuery query, CancellationToken ct)
        => Paged(await _ideas.SearchAsync(query, ct));

    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery] IdeaListQuery query, CancellationToken ct)
        => Paged(await _ideas.GetMyIdeasAsync(query, ct));

    [HttpGet("id/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var idea = await _ideas.GetByIdAsync(id, ct);
        return idea is null ? NotFoundError("Idea not found.") : OkData(idea);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var idea = await _ideas.GetBySlugAsync(slug, ct);
        return idea is null ? NotFoundError("Idea not found.") : OkData(idea);
    }

    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] IdeaCreateRequest request, CancellationToken ct)
        => OkResult(await _ideas.CreateAsync(request, ct));

    [HttpPost("quick")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateQuick([FromBody] QuickIdeaRequest request, CancellationToken ct)
        => OkResult(await _ideas.CreateQuickAsync(request, ct));

    [HttpPut("{id:int}")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] IdeaUpdateRequest request, CancellationToken ct)
        => OkResult(await _ideas.UpdateAsync(id, request, ct));

    [HttpPost("{id:int}/submit")]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
        => OkResult(await _ideas.SubmitAsync(id, ct));

    [HttpPost("{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, CancellationToken ct)
        => OkResult(await _ideas.WithdrawAsync(id, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDraft(int id, CancellationToken ct)
        => OkResult(await _ideas.DeleteDraftAsync(id, ct));

    [HttpGet("{id:int}/similar")]
    [AllowAnonymous]
    public async Task<IActionResult> Similar(int id, [FromQuery] int take = 5, CancellationToken ct = default)
        => OkData(await _ideas.GetSimilarAsync(id, take, ct));

    [HttpPost("{id:int}/view")]
    [AllowAnonymous]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> RecordView(int id, CancellationToken ct)
    {
        await _ideas.RecordViewAsync(id, ct);
        return OkData(new { recorded = true });
    }
}
