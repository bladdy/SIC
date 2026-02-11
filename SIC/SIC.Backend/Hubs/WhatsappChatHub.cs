using Microsoft.AspNetCore.SignalR;
using SIC.Shared.DTOs;

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
}