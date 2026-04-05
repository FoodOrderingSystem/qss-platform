using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace QSS.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public async Task JoinTeam(string teamName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, teamName);
        await Clients.Group(teamName).SendAsync("UserJoined", Context.UserIdentifier, teamName);
    }

    public async Task LeaveTeam(string teamName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamName);
    }

    public async Task SendTeamMessage(string teamName, string message)
    {
        await Clients.Group(teamName).SendAsync("ReceiveMessage", new
        {
            Content = message,
            SenderId = Context.UserIdentifier,
            TeamName = teamName,
            CreatedAt = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
