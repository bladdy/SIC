using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SIC.Backend.Data;
using SIC.Backend.Helpers;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
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
                string mensajeMostrar;

                if (!string.IsNullOrWhiteSpace(Message) && Message.Trim().StartsWith("{"))
                {
                    try
                    {
                        dynamic? errorObject = JsonConvert.DeserializeObject<dynamic>(Message);

                        string? mensajePrincipal = errorObject?.error?.message?.ToString();
                        string? detalle = errorObject?.error?.error_data?.details?.ToString();

                        mensajeMostrar = detalle ?? mensajePrincipal ?? "Error desconocido";
                    }
                    catch
                    {
                        mensajeMostrar = Message; // Si falla el parseo, devuelve el texto original
                    }
                }
                else
                {
                    mensajeMostrar = Message ?? "Error desconocido";
                }


                Console.WriteLine(mensajeMostrar);

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
                    Message = mensajeMostrar,
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

        public async Task<ActionResponse<bool>> AddReceiveMessages(WhatsappIncomingMessageDto whatsappIncoming)
        {
            var from = whatsappIncoming.From;
            // limpiar a solo números
            var fromDigits = new string(from.Where(char.IsDigit).ToArray());

            // últimos 10 dígitos
            var last10 = fromDigits.Length > 10
                ? fromDigits.Substring(fromDigits.Length - 10)
                : fromDigits;

            var guest = await _context.Invitations
                .Include(e => e.Event)
                .FirstOrDefaultAsync(x =>
                    x.PhoneNumber != null &&
                    x.PhoneNumber.Length >= 10 &&
                    x.PhoneNumber.Substring(x.PhoneNumber.Length - 10) == last10
                );
            var response = new ResponseFromWhatsApp
            {
                EventCode = guest?.Event?.Code,
                EventName = guest?.Event?.Name,
                NameConversation = guest?.Name,
                PhoneNumber = whatsappIncoming.PhoneNumber,
                From = whatsappIncoming.From,
                Message = whatsappIncoming.Text ?? string.Empty, // Use null-coalescing operator to provide a default value
                MessageId = whatsappIncoming.MessageId ?? string.Empty,
                Direction = whatsappIncoming.Direction,
                CreatedAt = DateTime.UtcNow,
                Type = whatsappIncoming.Type,
                Status = whatsappIncoming.Status//chk porque llega null
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

        public async Task<ActionResponse<IEnumerable<RealtimeChatMessageDto>>> GetConversationAsync(string phoneNumber)
        {
            var messages = await _context.ResponseFromWhatsApps
                .Where(x => x.From == phoneNumber)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new RealtimeChatMessageDto
                {
                    EventCode = x.EventCode!,
                    Name = x.NameConversation!,
                    MessageId = x.MessageId,
                    PhoneNumber = x.From,
                    Direction = x.Direction,
                    Type = x.Type,
                    Status = x.Status,
                    Content = x.Message,
                    Timestamp = x.CreatedAt
                })
                .ToListAsync();

            return new ActionResponse<IEnumerable<RealtimeChatMessageDto>>
            {
                Success = true,
                Result = messages
            };
        }

        //Tiene que marcar todos los mensajes de un psid como leidos
        public async Task<ActionResponse<bool>> MarkMessagesAsSeenAsync(string psid)
        {
            try
            {
                var messages = await _context.ResponseFromWhatsApps
                    .Where(x => x.MessageId == psid && x.Status != "seen")
                    .ToListAsync();
                foreach (var message in messages)
                {
                    message.Status = "seen";
                    _context.ResponseFromWhatsApps.Update(message);
                }
                await _context.SaveChangesAsync();
                return new ActionResponse<bool>
                {
                    Success = true,
                    Result = true
                };
            }
            catch (Exception)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = "No se pudieron marcar los mensajes como leidos."
                };
            }
        }

        public async Task<List<InboxConversationDto>> GetInboxAsync(string phoneNumber, string eventC)
        {
            var lastMessages = await _context.ResponseFromWhatsApps
                    .AsNoTracking()
                    .Where(m => m.PhoneNumber == phoneNumber && m.EventCode == eventC) // Filtrar por usuario
                    .GroupBy(m => new
                    {
                        m.From,
                        m.EventCode,
                        m.EventName,
                        m.NameConversation
                    })
                    .Select(g => g
                        .OrderByDescending(m => m.CreatedAt)
                        .First())
                    .ToListAsync(); // 👈 aquí se ejecuta en SQL

            return lastMessages
                .Select(m => new InboxConversationDto
                {
                    PhoneNumber = m.From,
                    EventCode = m.EventCode,
                    EventName = m.EventName,
                    NameConversation = m.NameConversation,

                    LastMessage = m.Message,
                    LastMessageAt = m.CreatedAt,
                    Direction = m.Direction,
                    Type = m.Type,

                    UnreadCount = _context.ResponseFromWhatsApps.Count(x =>
                        x.From == m.From &&
                        x.EventCode == m.EventCode &&
                        x.NameConversation == m.NameConversation &&
                        x.Direction == "IN" &&
                        !string.IsNullOrEmpty(x.MessageId) &&
                        //!string.IsNullOrEmpty(x.Status) &&
                        x.Status != "seen"
                    )
                })
                .OrderByDescending(x => x.LastMessageAt)
                .ToList();
        }

        public async Task<List<InboxConversationDto>> GetInboxAsync(string phoneNumber)
        {
            var lastMessages = await _context.ResponseFromWhatsApps
                .AsNoTracking()
                .Where(m => m.PhoneNumber != null && m.PhoneNumber == phoneNumber)
                .GroupBy(m => new
                {
                    m.EventCode
                })
                .Select(g => g
                    .OrderByDescending(m => m.CreatedAt)
                    .First())
                .ToListAsync();
            // 👈 aquí se ejecuta en SQL

            return lastMessages
                .Select(m => new InboxConversationDto
                {
                    PhoneNumber = m.From,
                    EventCode = m.EventCode,
                    EventName = m.EventName,
                    NameConversation = m.NameConversation,

                    LastMessage = m.Message,
                    LastMessageAt = m.CreatedAt,
                    Direction = m.Direction,
                    Type = m.Type,

                    UnreadCount = _context.ResponseFromWhatsApps.Count(x =>
                        x.EventCode == m.EventCode &&
                        x.Direction == "IN" &&
                        !string.IsNullOrEmpty(x.MessageId) &&
                        //!string.IsNullOrEmpty(x.Status) &&
                        x.Status != "seen"
                    )
                })
                .OrderByDescending(x => x.LastMessageAt)
                .ToList();
        }
    }
}