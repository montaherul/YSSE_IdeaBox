using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/notifications")]
public sealed class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _notifications.GetMineAsync(page, pageSize, ct));

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        => OkResult(await _notifications.MarkReadAsync(id, ct));

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        => OkResult(await _notifications.MarkAllReadAsync(ct));

    [HttpGet("preferences")]
    public async Task<IActionResult> Preferences(CancellationToken ct)
        => OkData(await _notifications.GetPreferencesAsync(ct));

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreference([FromBody] NotificationPreferenceUpdateRequest request, CancellationToken ct)
        => OkResult(await _notifications.UpdatePreferenceAsync(request, ct));
}
