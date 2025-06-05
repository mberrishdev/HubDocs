using Microsoft.AspNetCore.SignalR;

namespace HubDocs.Sample.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string title, string message, string type)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            Title = title,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SubscribeToUser(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    }

    public async Task UnsubscribeFromUser(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
    }
} 