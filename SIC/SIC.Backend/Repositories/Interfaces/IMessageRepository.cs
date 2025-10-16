using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Interfaces
{
    public interface IMessageRepository
    {
        Task<ActionResponse<Message>> GetByCodeAsync(string code);

        Task<ActionResponse<Message>> AddFullAsync(Message message, string eventCode);

        Task<ActionResponse<Message>> UpdateFullAsync(Message message, string eventCode);

        Task<ActionResponse<IEnumerable<MessageKey>>> GetKeysAsync();
    }
}