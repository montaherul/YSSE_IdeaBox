using Microsoft.AspNetCore.SignalR;
using YSSE.IdeaBox.Api.Hubs;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Api.Services;

/// <summary>
/// Pushes real-time events to a user's connected sessions over SignalR. Delivery
/// failures are logged and swallowed so they never affect the originating request.
/// </summary>
public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ILogger<SignalRRealtimeNotifier> _logger;

    public SignalRRealtimeNotifier(IHubContext<NotificationHub> hub, ILogger<SignalRRealtimeNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotifyUserAsync(int userId, string eventName, object payload, CancellationToken ct = default)
    {
        try
        {
            await _hub.Clients
                .Group(NotificationHub.GroupFor(userId))
                .SendAsync(eventName, payload, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push real-time '{EventName}' to user {UserId}.", eventName, userId);
        }
    }
}
