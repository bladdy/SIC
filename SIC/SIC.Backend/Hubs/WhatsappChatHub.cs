using Microsoft.AspNetCore.SignalR;
using SIC.Shared.DTOs;

namespace SIC.Backend.Hubs
{
    public class WhatsappChatHub : Hub
    {
        public async Task JoinChat(string phoneNumber)
            => await Groups.AddToGroupAsync(Context.ConnectionId, phoneNumber);

        public async Task LeaveChat(string phoneNumber)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, phoneNumber);

        public async Task SendToChat(string phoneNumber, RealtimeChatMessageDto message)
        => await Clients.Group(phoneNumber).SendAsync("ReceiveMessage", message);

        public async Task NotifyInboxUpdate(WhatsappInboxItemDto inbox)
        => await Clients.All.SendAsync("InboxUpdated", inbox);
    }
}