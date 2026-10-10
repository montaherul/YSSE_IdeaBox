using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
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

    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Create([FromBody] CategoryUpsertRequest request, CancellationToken ct)
        => OkResult(await _categories.CreateCategoryAsync(request, ct));

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpsertRequest request, CancellationToken ct)
        => OkResult(await _categories.UpdateCategoryAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _categories.DeleteCategoryAsync(id, ct));
}
