using Microsoft.AspNetCore.Mvc;
using SIC.Shared.Entities;
using SIC.Shared.Response;
using SIC.Shared.DTOs;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IMessageUnitOfWork
    {
        Task<ActionResponse<IEnumerable<MessageWhatsappInvitationDTO>>> GetMessageWhatsappInvitation(string code);

        Task<ActionResponse<Message>> GetByCodeAsync(string code);

        Task<ActionResponse<Message>> AddFullAsync(Message message, string eventCode);

        Task<ActionResponse<Message>> UpdateFullAsync(Message message, string eventCode);

        Task<ActionResponse<IEnumerable<MessageKey>>> GetKeysAsync();

        Task<ActionResponse<bool>> AddHistoryMessages(string code, bool Success, string? Message);

        Task<ActionResponse<IEnumerable<HistoryMessages>>> GetHistoryMessagesAsync();

        Task<ActionResponse<bool>> AddReceiveMessages(WhatsappIncomingMessageDto whatsappIncoming);

        Task<ActionResponse<IEnumerable<MessagesReciveDTO>>> GetAllMessagesReciveAsync();

        Task<ActionResponse<IEnumerable<RealtimeChatMessageDto>>> GetConversationAsync(string phoneNumber);

        Task<List<InboxConversationDto>> GetInboxAsync();
    }
}