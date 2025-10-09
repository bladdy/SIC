using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Interfaces;
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
    }

}