using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.UnitOfWork.Interfaces
{
    public interface IMessageUnitOfWork
    {
        Task<ActionResponse<Message>> GetByCodeAsync(string code);

        Task<ActionResponse<Message>> AddFullAsync(Message message, string eventCode);

        Task<ActionResponse<Message>> UpdateFullAsync(Message message, string eventCode);

        Task<ActionResponse<IEnumerable<MessageKey>>> GetKeysAsync();
    }
}