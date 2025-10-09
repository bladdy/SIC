using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations
{
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        private readonly DataContext _context;

        public MessageRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ActionResponse<Message>> GetByCodeAsync(string code)
        {
            var message = await _context.Messages.Include(e => e.Event).FirstOrDefaultAsync(x => x.Event!.Code!.Contains(code));
            if (message == null)
            {
                return new ActionResponse<Message>
                {
                    Success = true,
                    Message = "Este evento no tiene mesensajes de confirmacion e invitacion."
                };
            }
            return new ActionResponse<Message>
            {
                Success = true,
                Result = message
            };
        }

        public async Task<ActionResponse<Message>> AddFullAsync(Message message, string eventCode)
        {
            try
            {
                var Event = await _context.Events.FirstOrDefaultAsync(x => x.Code == eventCode);
                if (Event == null)
                {
                    return new ActionResponse<Message>
                    {
                        Success = false,
                        Message = "El evento no existe"
                    };
                }
                message.Event = Event;
                message.CreatedDate = DateTime.Now;
                _context.Add(message);
                await _context.SaveChangesAsync();
                return new ActionResponse<Message>
                {
                    Success = true,
                    Result = message
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<Message>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }

        public async Task<ActionResponse<Message>> UpdateFullAsync(Message message, string eventCode)
        {
            try
            {
                var Event = await _context.Events.FirstOrDefaultAsync(x => x.Code == eventCode);
                if (Event == null)
                {
                    return new ActionResponse<Message>
                    {
                        Success = false,
                        Message = "El evento no existe"
                    };
                }
                message.Event = Event;
                _context.Update(message);
                await _context.SaveChangesAsync();
                return new ActionResponse<Message>
                {
                    Success = true,
                    Result = message
                };
            }
            catch (Exception exception)
            {
                return new ActionResponse<Message>
                {
                    Success = false,
                    Message = exception.Message
                };
            }
        }
    }
}