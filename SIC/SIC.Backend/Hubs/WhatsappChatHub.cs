using Microsoft.AspNetCore.SignalR;
using SIC.Shared.DTOs;

namespace SIC.Backend.Hubs
{
    public class WhatsappChatHub : Hub
    {
        // 🔑 ÚNICA FUENTE DE VERDAD
        private static string ChatGroup(string phone, string contact)
            => $"chat-{phone}-{contact}";

        private static string EventInboxGroup(string phone, string eventCode)
            => $"event-inbox-{phone}-{eventCode}";

        // 💬 Chat activo
        public async Task JoinChat(string ownerPhone, string contactPhone)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                ChatGroup(ownerPhone, contactPhone)
            );
        }

        public async Task LeaveChat(string ownerPhone, string contactPhone)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                ChatGroup(ownerPhone, contactPhone)
            );
        }

        // 📥 Inbox por evento
        public async Task JoinEventInbox(string phone, string eventCode)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                EventInboxGroup(phone, eventCode)
            );
        }

        public async Task LeaveEventInbox(string phone, string eventCode)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                EventInboxGroup(phone, eventCode)
            );
        }

        // 📩 Enviar mensaje a un chat
        public async Task SendToChat(
            string phone,
            string contactPhone,
            RealtimeChatMessageDto message
        )
        {
            await Clients
                .Group(ChatGroup(phone, contactPhone))
                .SendAsync("NewMessage", message);
        }

        // 🔔 Notificar inbox SOLO al evento
        public async Task NotifyInboxUpdate(
            string phone,
            string eventCode,
            InboxConversationDto inbox
        )
        {
            await Clients
                .Group(EventInboxGroup(phone, eventCode))
                .SendAsync("InboxUpdated", inbox);
        }
    }
}