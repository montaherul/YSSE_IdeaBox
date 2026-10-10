using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YSSE.IdeaBox.Api.Requests;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1")]
public sealed class ConversationsController : ApiControllerBase
{
    private readonly IMessagingService _messaging;

    public ConversationsController(IMessagingService messaging) => _messaging = messaging;

    [HttpGet("conversations")]
    public async Task<IActionResult> List([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _messaging.GetConversationsAsync(page, pageSize, ct));

    [HttpGet("conversations/{id:int}")]
    public async Task<IActionResult> Get(int id, [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
    {
        var conversation = await _messaging.GetConversationAsync(id, page, pageSize, ct);
        return conversation is null ? NotFoundError("Conversation not found.") : OkData(conversation);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> Start([FromBody] StartConversationRequest request, CancellationToken ct)
        => OkResult(await _messaging.StartAsync(request, ct));

    [HttpPost("conversations/{id:int}/messages")]
    [EnableRateLimiting("messaging")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] MessageSendRequest request, CancellationToken ct)
        => OkResult(await _messaging.SendMessageAsync(id, request, ct));

    [HttpPost("conversations/{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        => OkResult(await _messaging.MarkReadAsync(id, ct));

    [HttpPost("conversations/{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] ConversationStatusRequest request, CancellationToken ct)
        => OkResult(await _messaging.SetStatusAsync(id, request.Status, ct));

    [HttpPost("users/{userId:int}/block")]
    public async Task<IActionResult> Block(int userId, CancellationToken ct)
        => OkResult(await _messaging.BlockUserAsync(userId, ct));

    [HttpPost("reports")]
    public async Task<IActionResult> Report([FromBody] ReportCreateRequest request, CancellationToken ct)
        => OkResult(await _messaging.ReportAsync(request, ct));
}
