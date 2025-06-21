using Microsoft.AspNetCore.SignalR;

namespace HubDocs.Sample.Hubs;

public interface IChatClient
{
    Task Connected(string connectionId);
    Task ReceiveMessage(string user, string message);
    Task UserJoined(string connectionId);
    Task UserLeft(string connectionId);
}

public class ChatHub : Hub<IChatClient>
{
    public async Task OnConnectedAsync(string user)
    {
        await Clients.Caller.Connected(Context.ConnectionId);
    }

    public async Task SendMessage(string user, string message)
    {
        await Clients.All.ReceiveMessage(user, message);
    }

    public async Task JoinRoom(string roomName, List<int> roles)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).UserJoined(Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).UserLeft(Context.ConnectionId);
    }
}