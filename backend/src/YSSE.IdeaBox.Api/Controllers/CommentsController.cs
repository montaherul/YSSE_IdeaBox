using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class CommentsController : ApiControllerBase
{
    private readonly ICommentService _comments;

    public CommentsController(ICommentService comments) => _comments = comments;

    [HttpGet("ideas/{ideaId:int}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> List(int ideaId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _comments.GetCommentsAsync(ideaId, page, pageSize, ct));

    [HttpPost("ideas/{ideaId:int}/comments")]
    [Authorize(Policy = Permissions.ReviewsCreate)]
    public async Task<IActionResult> Create(int ideaId, [FromBody] CommentCreateRequest request, CancellationToken ct)
        => OkResult(await _comments.AddCommentAsync(ideaId, request, ct));

    [HttpPut("comments/{id:int}")]
    [Authorize(Policy = Permissions.ReviewsUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] CommentUpdateRequest request, CancellationToken ct)
        => OkResult(await _comments.UpdateCommentAsync(id, request, ct));

    [HttpDelete("comments/{id:int}")]
    [Authorize(Policy = Permissions.ReviewsDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _comments.DeleteCommentAsync(id, ct));

    [HttpPost("comments/{id:int}/moderate")]
    [Authorize(Policy = Permissions.ReviewsModerate)]
    public async Task<IActionResult> Moderate(int id, [FromBody] ReviewModerateRequest request, CancellationToken ct)
        => OkResult(await _comments.ModerateCommentAsync(id, request, ct));

    [HttpPost("ideas/{ideaId:int}/follow")]
    [Authorize(Policy = Permissions.ReviewsRead)]
    public async Task<IActionResult> Follow(int ideaId, CancellationToken ct)
        => OkResult(await _comments.ToggleFollowAsync(ideaId, ct));

    [HttpGet("ideas/followed")]
    [Authorize(Policy = Permissions.ReviewsRead)]
    public async Task<IActionResult> Followed([FromQuery] IdeaListQuery query, CancellationToken ct)
        => Paged(await _comments.GetFollowedAsync(query, ct));
}