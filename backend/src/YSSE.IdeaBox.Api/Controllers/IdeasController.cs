using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
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
    [Authorize(Policy = Permissions.IdeasRead)]
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
    [Authorize(Policy = Permissions.IdeasCreate)]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] IdeaCreateRequest request, CancellationToken ct)
        => OkResult(await _ideas.CreateAsync(request, ct));

    [HttpPost("quick")]
    [Authorize(Policy = Permissions.IdeasCreate)]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> CreateQuick([FromBody] QuickIdeaRequest request, CancellationToken ct)
        => OkResult(await _ideas.CreateQuickAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.IdeasUpdate)]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Update(int id, [FromBody] IdeaUpdateRequest request, CancellationToken ct)
        => OkResult(await _ideas.UpdateAsync(id, request, ct));

    [HttpPost("{id:int}/submit")]
    [Authorize(Policy = Permissions.IdeasUpdate)]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Submit(int id, CancellationToken ct)
        => OkResult(await _ideas.SubmitAsync(id, ct));

    [HttpPost("{id:int}/withdraw")]
    [Authorize(Policy = Permissions.IdeasUpdate)]
    public async Task<IActionResult> Withdraw(int id, CancellationToken ct)
        => OkResult(await _ideas.WithdrawAsync(id, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.IdeasDelete)]
    public async Task<IActionResult> DeleteDraft(int id, CancellationToken ct)
        => OkResult(await _ideas.DeleteDraftAsync(id, ct));

    /// <summary>Uploads/replaces the private PDF containing the full idea description (owner only).</summary>
    [HttpPost("{id:int}/attachment")]
    [Authorize(Policy = Permissions.IdeasUpdate)]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UploadAttachment(int id, [FromForm] IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Failed("Select a PDF document to upload.");
        }

        await using var stream = file.OpenReadStream();
        return OkResult(await _ideas.UploadAttachmentAsync(id, stream, file.FileName, file.ContentType, file.Length, ct));
    }

    [HttpDelete("{id:int}/attachment")]
    [Authorize(Policy = Permissions.IdeasUpdate)]
    public async Task<IActionResult> DeleteAttachment(int id, CancellationToken ct)
        => OkResult(await _ideas.RemoveAttachmentAsync(id, ct));

    /// <summary>Streams the private idea document; allowed for the owner or holders of the attachment read permission.</summary>
    [HttpGet("{id:int}/attachment")]
    [Authorize]
    public async Task<IActionResult> DownloadAttachment(int id, CancellationToken ct)
    {
        var download = await _ideas.OpenAttachmentAsync(id, ct);
        return download is null
            ? NotFoundError("No document is attached to this idea.")
            : File(download.Content, download.ContentType, download.FileName);
    }

    [HttpPost("{id:int}/feature")]
    [Authorize(Policy = Permissions.IdeasFeature)]
    public async Task<IActionResult> Feature(int id, CancellationToken ct)
        => OkResult(await _ideas.SetFeaturedAsync(id, true, ct));

    [HttpPost("{id:int}/unfeature")]
    [Authorize(Policy = Permissions.IdeasFeature)]
    public async Task<IActionResult> Unfeature(int id, CancellationToken ct)
        => OkResult(await _ideas.SetFeaturedAsync(id, false, ct));

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