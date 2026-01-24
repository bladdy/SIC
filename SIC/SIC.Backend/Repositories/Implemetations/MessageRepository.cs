using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;
using System.Collections.Generic;

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

        public async Task<ActionResponse<IEnumerable<MessageKey>>> GetKeysAsync()
        {
            var entities = await _context.MessageKeys.ToListAsync();
            return new ActionResponse<IEnumerable<MessageKey>>
            {
                Success = true,
                Result = entities
            };
        }

        public async Task<ActionResponse<IEnumerable<MessageWhatsappInvitationDTO>>> GetMessageWhatsappInvitation(string code)
        {
            var invitations = await _context.Invitations
                .Include(i => i.Event)
                .Where(i => i.Event!.Code == code && i.Status == Shared.Enums.Status.Pending)
                .ToListAsync();

            var message = await _context.Messages
                .Include(e => e.Event)
                .FirstOrDefaultAsync(x => x.Event!.Code!.Contains(code));

            var key = await _context.MessageKeys.ToListAsync();

            // Generamos el mensaje formateado para cada invitación
            var messageWhatsappInvitations = invitations.Select(invitation =>
            {
                var formattedMessage = MessageFormatter.FormatMessage(message!, invitation, key.ToList());

                return new MessageWhatsappInvitationDTO
                {
                    Name = invitation.Name,
                    Event = invitation.Event!.Name,
                    PhoneNumber = invitation.PhoneNumber,  // O la propiedad correcta para el número
                    MessageConfirmation = formattedMessage.MessageConfirmation,
                    MessageInvitation = formattedMessage.MessageInvitation,
                    Sent = false,  // Se puede modificar cuando se marque como enviado
                    Error = string.Empty  // Se puede actualizar en caso de error al enviar
                };
            }).ToList();

            return new ActionResponse<IEnumerable<MessageWhatsappInvitationDTO>>
            {
                Success = true,
                Result = messageWhatsappInvitations
            };
        }

        public async Task<ActionResponse<bool>> AddHistoryMessages(string code, bool Success, string? Message)
        {
            try
            {
                var invitations = await _context.Invitations
                    .Include(i => i.Event)
                    .FirstOrDefaultAsync(i => i.Code == code);
                if (invitations == null)
                {
                    return new ActionResponse<bool>
                    {
                        Success = false,
                        Message = "La invitación no existe."
                    };
                }
                var mesage = new HistoryMessages
                {
                    Invitation = invitations,
                    Event = invitations.Event!,
                    Delivered = Success,
                    SendDate = DateTime.Now,
                    Send = Success,
                    Error = !Success,
                    //Message = Message,
                };
                _context.Add(mesage);
                await _context.SaveChangesAsync();
                return new ActionResponse<bool>
                {
                    Success = true,
                    Message = "La History Messages."
                };
            }
            catch (Exception)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "Algo paso."
                };
            }
        }

        public async Task<ActionResponse<IEnumerable<HistoryMessages>>> GetHistoryMessagesAsync()
        {
            var message = await _context.HistoryMessages
               .Include(e => e.Event)
               .Include(e => e.Invitation).ToListAsync();

            return new ActionResponse<IEnumerable<HistoryMessages>>
            {
                Success = true,
                Result = message
            };
        }

        public async Task<ActionResponse<bool>> AddReceiveMessages(string from, string? text, string? replyToMessageId)
        {
            var response = new ResponseFromWhatsApp
            {
                From = from,
                Message = text ?? string.Empty, // Use null-coalescing operator to provide a default value
                MessageId = replyToMessageId ?? string.Empty // Use null-coalescing operator to provide a default value
            };
            _context.Add(response);
            await _context.SaveChangesAsync();
            return new ActionResponse<bool>
            {
                Success = true,
                Message = "Funcionalidad no implementada."
            };
        }

        public async Task<ActionResponse<IEnumerable<MessagesReciveDTO>>> GetAllMessagesReciveAsync()
        {
            var messages = await (
                from r in _context.ResponseFromWhatsApps
                join i in _context.Invitations
                    on r.From.Substring(r.From.Length - 10) equals i.PhoneNumber
                orderby r.CreatedAt descending
                select new MessagesReciveDTO
                {
                    InvitationName = i.Name,
                    InvitationCode = i.Code!,
                    EventName = i.Event!.Name,
                    Message = r.Message,
                    From = r.From,
                    CreatedAt = r.CreatedAt
                }
            ).ToListAsync();

            return new ActionResponse<IEnumerable<MessagesReciveDTO>>
            {
                Success = true,
                Result = messages
            };
        }
    }
}