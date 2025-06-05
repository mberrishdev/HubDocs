using Microsoft.AspNetCore.SignalR;

namespace HubDocs.Sample.Hubs;

public class ChatHub : Hub
{
    public async Task OnConnectedAsync(string user)
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
    }
    
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task JoinRoom(string roomName, List<int> roles)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserJoined", Context.ConnectionId);
    }

    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserLeft", Context.ConnectionId);
    }
} 