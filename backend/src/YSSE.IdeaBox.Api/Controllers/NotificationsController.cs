using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/notifications")]
[Authorize(Policy = Permissions.NotificationsRead)]
public sealed class NotificationsController : ApiControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _notifications.GetMineAsync(page, pageSize, ct));

    [HttpPost("{id:int}/read")]
    [Authorize(Policy = Permissions.NotificationsManage)]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        => OkResult(await _notifications.MarkReadAsync(id, ct));

    [HttpPost("read-all")]
    [Authorize(Policy = Permissions.NotificationsManage)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        => OkResult(await _notifications.MarkAllReadAsync(ct));

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Permissions.NotificationsDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => OkResult(await _notifications.DeleteAsync(id, ct));

    [HttpGet("preferences")]
    public async Task<IActionResult> Preferences(CancellationToken ct)
        => OkData(await _notifications.GetPreferencesAsync(ct));

    [HttpPut("preferences")]
    [Authorize(Policy = Permissions.NotificationsManage)]
    public async Task<IActionResult> UpdatePreference([FromBody] NotificationPreferenceUpdateRequest request, CancellationToken ct)
        => OkResult(await _notifications.UpdatePreferenceAsync(request, ct));
}