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

        public async Task<ActionResponse<bool>> AddHistoryMessages(
            string code,
            bool Success,
            string? Message,
            WhatsAppMessageResponse messageResponse)
        {
            try
            {
                var invitation = await _context.Invitations
                    .Include(i => i.Event)
                    .FirstOrDefaultAsync(i => i.Code == code);

                if (invitation == null)
                {
                    return new ActionResponse<bool>
                    {
                        Success = false,
                        Message = "La invitación no existe."
                    };
                }

                string mensajeMostrar = Message ?? string.Empty;

                if (!Success)
                {
                    if (!string.IsNullOrWhiteSpace(Message) &&
                        Message.Trim().StartsWith("{"))
                    {
                        try
                        {
                            dynamic? errorObject =
                                JsonConvert.DeserializeObject<dynamic>(Message);

                            string? mensajePrincipal =
                                errorObject?.error?.message?.ToString();

                            string? detalle =
                                errorObject?.error?.error_data?.details?.ToString();

                            mensajeMostrar =
                                detalle ??
                                mensajePrincipal ??
                                "Error desconocido";
                        }
                        catch
                        {
                            mensajeMostrar = Message;
                        }
                    }
                    else
                    {
                        mensajeMostrar = Message ?? "Error desconocido";
                    }
                }

                var history = new HistoryMessages
                {
                    InvitationId = invitation.Id,
                    EventId = invitation.EventId,

                    SendDate = DateTime.UtcNow,

                    // Estado inicial al enviar
                    Send = Success,

                    // Estos estados se actualizarán desde el webhook
                    Delivered = false,
                    Read = false,

                    Error = !Success,

                    // wamid retornado por WhatsApp
                    MessageId = messageResponse?.Wamid,

                    ErrorMessage = !Success
                        ? mensajeMostrar
                        : null,

                    Message = Success
                        ? "Plantilla enviada correctamente"
                        : mensajeMostrar
                };

                _context.HistoryMessages.Add(history);

                await _context.SaveChangesAsync();

                return new ActionResponse<bool>
                {
                    Success = true,
                    Message = "Historial guardado correctamente."
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ActionResponse<IEnumerable<HistoryMessages>>> GetHistoryMessagesAsync(PaginationDTO pagination)
        {
            var queryable = _context.HistoryMessages
                .Include(e => e.Event)
                .Include(e => e.Invitation)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x =>
                    x.Event.Name.Contains(pagination.Filter));
            }

            return new ActionResponse<IEnumerable<HistoryMessages>>
            {
                Success = true,
                Result = await queryable
                    .OrderByDescending(e => e.SendDate)
                    .Paginate(pagination)
                    .ToListAsync()
            };
        }

        public async Task<ActionResponse<int>> GetHistoryMessagesTotalRecordAsync(PaginationDTO pagination)
        {
            var queryable = _context.HistoryMessages
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x =>
                    x.Event != null &&
                    x.Event.Name.Contains(pagination.Filter));
            }

            queryable = queryable.Where(e => e.Invitation != null && e.Event != null);

            var count = await queryable.CountAsync();

            int totalPages = (int)Math.Ceiling((double)count / pagination.PageSize);

            return new ActionResponse<int>
            {
                Success = true,
                Result = totalPages
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
            // Buscar la invitación que coincida con el número de teléfono (últimos 10 dígitos)
            var guest = await _context.Invitations
                .Include(e => e.Event)
                .Where(x =>
                    x.PhoneNumber != null &&
                    x.PhoneNumber.Length >= 10 &&
                    x.PhoneNumber.Substring(x.PhoneNumber.Length - 10) == last10
                )
                .OrderByDescending(x => x.Event!.Date) // 👈 el más reciente primero
                .FirstOrDefaultAsync();
            //Validar si Direction = "IN" busque el ultimo mensaje que se le envio a ese numero y evento para obtener el EventCode, EventName y NameConversation
            //En ResponseFromWhatsApp se guardara el EventCode, EventName y NameConversation para luego mostrarlo en el inbox
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
                Status = whatsappIncoming.Status,//chk porque llega null
                Imagen = whatsappIncoming.Imagen
            };
            _context.Add(response);
            await _context.SaveChangesAsync();
            return new ActionResponse<bool>
            {
                Success = true,
                Message = "Mensaje Guardado."
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
                    Imagen = x.Imagen,
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
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                    return new List<InboxConversationDto>();

                var baseQuery = _context.ResponseFromWhatsApps
                    .AsNoTracking()
                    .Where(m => m.PhoneNumber != null && m.PhoneNumber == phoneNumber);

                var lastMessages = await baseQuery
                    .GroupBy(m => m.EventCode)
                    .Select(g => g
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault())
                    .ToListAsync();

                if (lastMessages == null || !lastMessages.Any())
                    return new List<InboxConversationDto>();

                // Obtener conteos en una sola consulta
                var unreadCounts = await baseQuery
                    .Where(x =>
                        x.EventCode != null &&   // 🔥 FIX
                        x.Direction == "IN" &&
                        !string.IsNullOrEmpty(x.MessageId) &&
                        x.Status != "seen")
                    .GroupBy(x => x.EventCode)
                    .Select(g => new
                    {
                        EventCode = g.Key!,
                        Count = g.Count()
                    })
                    .ToDictionaryAsync(x => x.EventCode, x => x.Count);

                return lastMessages
                    .Where(m => m != null)
                    .Select(m => new InboxConversationDto
                    {
                        PhoneNumber = m.From ?? "",
                        EventCode = m.EventCode ?? "",
                        EventName = m.EventName ?? "",
                        NameConversation = m.NameConversation ?? "",

                        LastMessage = m.Message ?? "",
                        LastMessageAt = m.CreatedAt,
                        Direction = m.Direction ?? "",
                        Type = m.Type ?? "",

                        UnreadCount = unreadCounts.ContainsKey(m.EventCode)
                            ? unreadCounts[m.EventCode]
                            : 0
                    })
                    .OrderByDescending(x => x.LastMessageAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener el inbox: {ex.Message}");
                return new List<InboxConversationDto>();
            }
        }

        public async Task UpdateStatusAsync(
            string messageId,
            string status,
            string? errorCode = null)
        {
            var history = await _context.HistoryMessages
                .FirstOrDefaultAsync(x => x.MessageId == messageId);

            if (history == null)
                return;

            switch (status?.ToLower())
            {
                case "accepted":
                case "sent":
                    history.Send = true;
                    history.Message = "Mensaje enviado correctamente.";
                    break;

                case "delivered":
                    history.Delivered = true;
                    history.Message = "Mensaje entregado al destinatario.";
                    break;

                case "read":
                    history.Read = true;
                    history.Message = "Mensaje leído por el destinatario.";
                    break;

                case "failed":
                    history.Error = true;
                    history.ErrorCode = errorCode;
                    history.ErrorMessage = GetMetaErrorMessage(errorCode);
                    history.Message = history.ErrorMessage;
                    break;
            }

            await _context.SaveChangesAsync();
        }

        private static string GetMetaErrorMessage(string? errorCode)
        {
            return errorCode switch
            {
                "131026" => "No fue posible entregar el mensaje. El número puede no tener WhatsApp o no estar disponible.",

                "131031" => "La cuenta de WhatsApp Business está restringida o bloqueada.",

                "131042" => "Existe un problema relacionado con la facturación o pagos de la cuenta.",

                "131047" => "No se puede enviar el mensaje porque la conversación ha expirado (ventana de 24 horas cerrada).",

                "131049" => "Meta decidió no entregar el mensaje para proteger la experiencia del usuario.",

                "131050" => "El usuario ha dejado de recibir mensajes de marketing.",

                "131051" => "El tipo de mensaje enviado no es compatible.",

                "131052" => "No fue posible descargar el archivo multimedia.",

                "131053" => "No fue posible cargar el archivo multimedia.",

                "131056" => "Se alcanzó el límite de mensajes permitidos para este destinatario.",

                "131060" => "El mensaje solicitado ya no está disponible.",

                "131064" => "Se alcanzó un límite relacionado con la calidad o clasificación de plantillas.",

                "130429" => "Se excedió el límite de velocidad permitido por WhatsApp.",

                "130497" => "No es posible enviar mensajes al país de destino debido a restricciones.",

                "131021" => "El remitente y destinatario no pueden ser el mismo número.",

                null => "Error desconocido reportado por WhatsApp.",

                _ => $"Error de WhatsApp ({errorCode}). Consulte la documentación de Meta para más detalles."
            };
        }
    }
}