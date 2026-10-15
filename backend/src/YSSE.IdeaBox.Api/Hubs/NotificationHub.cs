using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace YSSE.IdeaBox.Api.Hubs;

/// <summary>
/// Authenticated real-time channel for a user's in-app notifications. Each connection
/// joins a per-user group so messages are only delivered to that user's sessions.
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
    public static string GroupFor(int userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var subject = Context.User?.FindFirst("sub")?.Value;
        if (int.TryParse(subject, out var userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupFor(userId));
        }

        await base.OnConnectedAsync();
    }
}
