using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SIC.Backend.Hubs;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Enums;
using System.Security.Claims;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsappWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMessageUnitOfWork _iMessageUnitOfWork;
    private readonly IHubContext<WhatsappChatHub> _hub;
    private readonly IUsuarioWhatsAppConfigUnitOfWork _repo;
    private readonly IWhatsAppConfigUnitOfWork _whatsAppConfigUnitOfWork;

    public WhatsappWebhookController(IConfiguration configuration, IMessageUnitOfWork iMessageUnitOfWork, IHubContext<WhatsappChatHub> hub, IUsuarioWhatsAppConfigUnitOfWork repo, IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork)
    {
        _hub = hub;
        _iMessageUnitOfWork = iMessageUnitOfWork;
        _configuration = configuration;
        _repo = repo;
        _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
    }

    // 🔐 Verificación inicial de Meta
    [HttpGet()]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var accessToken = _configuration["WhatsApp:invboxv_webhook_token"];//Para que venga del coonfig

        if (mode == "subscribe" && token == accessToken)
        {
            return Ok(challenge);
        }

        return Unauthorized();
    }

    // ✅ POST con ID en la ruta
    [HttpPost]
    public async Task<IActionResult> Receive(
    [FromBody] WhatsappWebhookPayload payload)
    {
        try
        {
            var changes = payload.Entry?
                .SelectMany(e => e.Changes)
                .ToList();

            if (changes == null || !changes.Any())
                return Ok();

            foreach (var change in changes)
            {
                var value = change.Value;

                // 🔑 Identifica el número de WhatsApp del cliente
                var phoneNumberId = value.Metadata?.PhoneNumberId;

                if (string.IsNullOrWhiteSpace(phoneNumberId))
                    continue;

                var owner = await _repo.GetByPhoneNumberIdAsync(phoneNumberId);

                if (owner == null)
                    continue;

                var messages = value.Messages ?? new List<MessageDTO>();
                var statuses = value.Statuses ?? new List<MessageStatus>();

                #region ESTADOS DE MENSAJES (sent, delivered, read, failed)

                foreach (var status in statuses)
                {
                    try
                    {
                        var error = status.Errors?.FirstOrDefault();

                        var errorCode = error?.Code.ToString();
                        var errorTitle = error?.Title;

                        Console.WriteLine(
                            $"Status: {status.Status} - Error: {errorCode} - {errorTitle}");

                        // Actualiza HistoryMessages
                        await _iMessageUnitOfWork.UpdateStatusAsync(
                            status.Id,
                            status.Status, errorCode);

                        // Actualiza tabla de mensajes/chat
                        /*await _iMessageUnitOfWork.UpdateMessageStatusAsync(
                            status.Id,
                            status.Status);*/

                        // Notifica a clientes conectados
                        await _hub.Clients
                            .Group($"message-status-{owner.PhoneNumber}")
                            .SendAsync(
                                "MessageStatusUpdated",
                                new
                                {
                                    MessageId = status.Id,
                                    Status = status.Status
                                });

                        Console.WriteLine(
                            $"Estado actualizado: {status.Id} -> {status.Status}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Error procesando status {status.Id}: {ex.Message}");
                    }
                }

                #endregion ESTADOS DE MENSAJES (sent, delivered, read, failed)

                #region MENSAJES ENTRANTES

                foreach (var message in messages)
                {
                    try
                    {
                        var status = statuses
                            .FirstOrDefault(s => s.Id == message.Id);

                        var dto = new WhatsappIncomingMessageDto
                        {
                            MessageId = message.Id,
                            From = message.From,
                            PhoneNumberId = phoneNumberId,
                            PhoneNumber = owner.PhoneNumber,
                            Text = message.Text?.Body,
                            Type = message.Type,
                            ReplyToMessageId = message.Context?.Id,
                            Timestamp = DateTime.UtcNow,
                            Direction = "IN",
                            Status = status?.Status ?? "received"
                        };

                        // Guardar mensaje
                        await _iMessageUnitOfWork.AddReceiveMessages(dto);

                        var conversation =
                            await _iMessageUnitOfWork.GetConversationAsync(dto.From);

                        var eventCode =
                            conversation?.Result?.LastOrDefault()?.EventCode
                            ?? "default";

                        // Actualizar Inbox
                        await _hub.Clients
                            .Group($"event-inbox-{owner.PhoneNumber}-{eventCode}")
                            .SendAsync(
                                "InboxUpdated",
                                new InboxConversationDto
                                {
                                    EventCode = eventCode,
                                    PhoneNumber = dto.From,
                                    LastMessage = dto.Text ?? string.Empty,
                                    LastMessageAt = dto.Timestamp,
                                    UnreadCount = 1
                                });

                        // Enviar mensaje en tiempo real al chat abierto
                        await _hub.Clients
                            .Group($"chat-{owner.PhoneNumber}-{dto.From}")
                            .SendAsync(
                                "NewMessage",
                                new RealtimeChatMessageDto
                                {
                                    MessageId = dto.MessageId,
                                    PhoneNumber = dto.From,
                                    Direction = "IN",
                                    MessageType = dto.Type,
                                    Content = dto.Text,
                                    Timestamp = dto.Timestamp,
                                    Status = dto.Status
                                });

                        Console.WriteLine(
                            $"Mensaje recibido de {dto.From}: {dto.Text}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Error procesando mensaje: {ex.Message}");
                    }
                }

                #endregion MENSAJES ENTRANTES
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Webhook Error: {ex}");

            return Ok(); // WhatsApp espera 200 para evitar reintentos innecesarios
        }
    }

    [HttpGet("chat/{phoneNumber}")]
    public async Task<IActionResult> GetChat(string phoneNumber)
    {
        var messages = await _iMessageUnitOfWork
            .GetConversationAsync(phoneNumber);

        return Ok(messages);
    }

    [HttpGet("whatsapp/inbox/{eventC}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetInbox(string eventC)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (usuarioId == null)
            return Unauthorized();
        var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(usuarioId);
        if (userWhatsAppConfig.Result == null)
            return NotFound();
        var inbox = await _iMessageUnitOfWork.GetInboxAsync(userWhatsAppConfig.Result.PhoneNumber!, eventC);
        return Ok(inbox);
    }

    [HttpGet("whatsapp/inbox")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> GetInbox()
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (usuarioId == null)
            return Unauthorized();
        var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(usuarioId);
        if (userWhatsAppConfig.Result == null)
            return NotFound();
        var inbox = await _iMessageUnitOfWork.GetInboxAsync(userWhatsAppConfig.Result.PhoneNumber!);
        return Ok(inbox);
    }
}