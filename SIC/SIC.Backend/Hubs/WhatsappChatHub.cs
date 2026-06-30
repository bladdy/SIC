using Microsoft.AspNetCore.SignalR;

namespace SIC.Backend.Hubs;

public class WhatsappChatHub : Hub
{
    public async Task JoinChat(string ownerPhone, string contactPhone)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            SignalRGroups.Chat(ownerPhone, contactPhone)
        );
    }

    public async Task LeaveChat(string ownerPhone, string contactPhone)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            SignalRGroups.Chat(ownerPhone, contactPhone)
        );
    }

    public async Task JoinEventInbox(string ownerPhone, string eventCode)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            SignalRGroups.EventInbox(ownerPhone, eventCode)
        );
    }

    public async Task LeaveEventInbox(string ownerPhone, string eventCode)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            SignalRGroups.EventInbox(ownerPhone, eventCode)
        );
    }

    public async Task JoinUserNotifications(string userId)
    {
        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"user-{userId}"
        );
    }

    public async Task JoinNotifications(string userId)
    {
        Console.WriteLine($"🔥 HUB JOIN: {userId}");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"notifications-{userId}");
    }

    public async Task LeaveUserNotifications(string userId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"user-{userId}"
        );
    }
}