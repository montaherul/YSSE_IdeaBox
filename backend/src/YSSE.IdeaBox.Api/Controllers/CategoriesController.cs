using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/categories")]
public sealed class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories) => _categories = categories;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List([FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => OkData(await _categories.GetCategoriesAsync(includeInactive, ct));

    [HttpGet("/api/v1/tags")]
    [AllowAnonymous]
    public async Task<IActionResult> Tags(CancellationToken ct)
        => OkData(await _categories.GetTagsAsync(ct));

    [HttpGet("tags")]
    [Authorize(Policy = Permissions.TagsRead)]
    public async Task<IActionResult> TagsPaged([FromQuery] int page = 1, [FromQuery] int pageSize = Pagination.DefaultPageSize, [FromQuery] string? search = null, CancellationToken ct = default)
        => Paged(await _categories.GetTagsPagedAsync(page, pageSize, search, ct));

    [HttpPost]
    [Authorize(Policy = Permissions.CategoriesCreate)]
    public async Task<IActionResult> Create([FromBody] CategoryUpsertRequest request, CancellationToken ct)
        => OkResult(await _categories.CreateCategoryAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = Permissions.CategoriesUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpsertRequest request, CancellationToken ct)
        => OkResult(await _categories.UpdateCategoryAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.CategoriesDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _categories.DeleteCategoryAsync(id, ct));

    [HttpPost("tags")]
    [Authorize(Policy = Permissions.TagsCreate)]
    public async Task<IActionResult> CreateTag([FromBody] TagUpsertRequest request, CancellationToken ct)
        => OkResult(await _categories.CreateTagAsync(request, ct));

    [HttpPut("tags/{id:int}")]
    [Authorize(Policy = Permissions.TagsUpdate)]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] TagUpsertRequest request, CancellationToken ct)
        => OkResult(await _categories.UpdateTagAsync(id, request, ct));

    [HttpDelete("tags/{id:int}")]
    [Authorize(Policy = Permissions.TagsDelete)]
    public async Task<IActionResult> DeleteTag(int id, CancellationToken ct)
        => OkResult(await _categories.DeleteTagAsync(id, ct));
}