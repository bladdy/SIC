using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Implemetations
{
    public class MessageUnitOfWork : GenericUnitOfWork<Message>, IMessageUnitOfWork
    {
        private readonly IMessageRepository _messageRepository;

        public MessageUnitOfWork(IGenericRepository<Message> repository, IMessageRepository messageRepository) : base(repository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<ActionResponse<Message>> GetByCodeAsync(string code) => await _messageRepository.GetByCodeAsync(code);

        public async Task<ActionResponse<Message>> AddFullAsync(Message message, string eventCode) => await _messageRepository.AddFullAsync(message, eventCode);

        public async Task<ActionResponse<Message>> UpdateFullAsync(Message message, string eventCode) => await _messageRepository.UpdateFullAsync(message, eventCode);

        public async Task<ActionResponse<IEnumerable<MessageKey>>> GetKeysAsync() => await _messageRepository.GetKeysAsync();

        public async Task<ActionResponse<IEnumerable<MessageWhatsappInvitationDTO>>> GetMessageWhatsappInvitation(string code) => await _messageRepository.GetMessageWhatsappInvitation(code);

        public async Task<ActionResponse<bool>> AddHistoryMessages(string code, bool Success, string? Message, WhatsAppMessageResponse messageResponse) => await _messageRepository.AddHistoryMessages(code, Success, Message, messageResponse);

        public async Task<ActionResponse<IEnumerable<HistoryMessages>>> GetHistoryMessagesAsync(PaginationDTO pagination) => await _messageRepository.GetHistoryMessagesAsync(pagination);

        public async Task<ActionResponse<int>> GetHistoryMessagesTotalRecordAsync(PaginationDTO pagination) => await _messageRepository.GetHistoryMessagesTotalRecordAsync(pagination);

        public async Task<ActionResponse<bool>> AddReceiveMessages(WhatsappIncomingMessageDto whatsappIncoming) => await _messageRepository.AddReceiveMessages(whatsappIncoming);

        public async Task<ActionResponse<IEnumerable<MessagesReciveDTO>>> GetAllMessagesReciveAsync() => await _messageRepository.GetAllMessagesReciveAsync();

        public async Task<ActionResponse<IEnumerable<RealtimeChatMessageDto>>> GetConversationAsync(string phoneNumber) => await _messageRepository.GetConversationAsync(phoneNumber);

        public async Task<ActionResponse<bool>> MarkMessagesAsSeenAsync(string Psid) => await _messageRepository.MarkMessagesAsSeenAsync(Psid);

        public async Task<List<InboxConversationDto>> GetInboxAsync(string usuarioId) => await _messageRepository.GetInboxAsync(usuarioId);

        public async Task<List<InboxConversationDto>> GetInboxAsync(string phoneNumber, string eventC) => await _messageRepository.GetInboxAsync(phoneNumber, eventC);

        public async Task UpdateStatusAsync(string id, string status) => await _messageRepository.UpdateStatusAsync(id, status);
    }
}