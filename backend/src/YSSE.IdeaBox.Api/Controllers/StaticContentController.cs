using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/static-content")]
[Authorize]
public sealed class StaticContentController : ApiControllerBase
{
    private readonly IStaticContentService _staticContent;

    public StaticContentController(IStaticContentService staticContent) => _staticContent = staticContent;

    [HttpGet]
    [Authorize(Policy = Permissions.StaticContentRead)]
    public async Task<IActionResult> GetStaticContents([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Paged(await _staticContent.GetStaticContentsAsync(page, pageSize, search, ct));

    [HttpPost]
    [Authorize(Policy = Permissions.StaticContentManage)]
    public async Task<IActionResult> CreateStaticContent([FromBody] StaticContentUpsertRequest request, CancellationToken ct)
        => OkResult(await _staticContent.CreateStaticContentAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.StaticContentManage)]
    public async Task<IActionResult> UpdateStaticContent(int id, [FromBody] StaticContentUpsertRequest request, CancellationToken ct)
        => OkResult(await _staticContent.UpdateStaticContentAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.StaticContentManage)]
    public async Task<IActionResult> DeleteStaticContent(int id, CancellationToken ct)
        => OkResult(await _staticContent.DeleteStaticContentAsync(id, ct));

    [HttpGet("{key}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStaticContentByKey(string key, CancellationToken ct)
        => OkData(await _staticContent.GetStaticContentByKeyAsync(key, ct));
}