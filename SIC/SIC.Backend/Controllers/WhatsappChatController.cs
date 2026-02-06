using Azure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SIC.Backend.Hubs;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.Security.Claims;
using static QRCoder.PayloadGenerator;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/whatsapp/chat")]
    public class WhatsappChatController : ControllerBase
    {
        private readonly IMessageUnitOfWork _messageUnitOfWork;
        private readonly WhatsAppService _whatsAppService;
        private readonly IWhatsAppConfigUnitOfWork _whatsAppConfigUnitOfWork;
        private readonly IHubContext<WhatsappChatHub> _hub;

        public WhatsappChatController(
            WhatsAppService whatsAppService, IMessageUnitOfWork messageUnitOfWork, IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork, IHubContext<WhatsappChatHub> hub)
        {
            _messageUnitOfWork = messageUnitOfWork;
            _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
            _whatsAppService = whatsAppService;
            _hub = hub;
        }

        [HttpGet("{phoneNumber}")]
        public async Task<IActionResult> GetChat(string phoneNumber)
        {
            var response = await _messageUnitOfWork.GetConversationAsync(phoneNumber);
            return Ok(response);
        }

        [HttpPut("mark-seen")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> MarkMessagesAsSeen([FromBody] MarkMessagesAsSeenDto dto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
                if (!userWhatsAppConfig.Success)
                    return BadRequest(new { error = "Este usuario no tiene permisos para hacer este envio" });
                //obtener los datos del usuario
                var accessToken = userWhatsAppConfig.Result!.AccessToken;
                var phoneNumberId = userWhatsAppConfig.Result!.PhoneNumberId;
                if (dto == null)
                    return BadRequest(new { error = "El PSID es requerido" });
                var marked = await _whatsAppService.MarkMessagesAsSeenAsync(
                accessToken!, phoneNumberId!, dto);
                if (!marked)
                    return BadRequest(new { error = "No se pudieron marcar los mensajes como leidos en WhatsApp" });
                var response = await _messageUnitOfWork.MarkMessagesAsSeenAsync(dto.Psid);
                if (!response.Success)
                    return BadRequest(new { error = "No se pudieron marcar los mensajes como leidos" });
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("templates")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetTemplates()
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });
            var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
            if (!userWhatsAppConfig.Success)
                return BadRequest(new { error = "Este usuario no tiene permisos para obtener las plantillas" });

            var templates = await _whatsAppService.GetTemplatesAsync(userWhatsAppConfig.Result);

            return Ok(templates);
        }

        [HttpPost("send")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> SendMessage([FromBody] SendWhatsappMessageDto dto)
        {
            try
            {
                // Extraer el ID del usuario autenticado desde el token JWT
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
                if (!userWhatsAppConfig.Success)
                    return BadRequest(new { error = "Este usuario no tiene permisos para hacer este envio" });

                //obtener los datos del usuario
                var accessToken = userWhatsAppConfig.Result!.AccessToken;
                var phoneNumberId = userWhatsAppConfig.Result!.PhoneNumberId;
                var wamid = await _whatsAppService.SendTextMessageAsync(
                accessToken!, phoneNumberId!, dto);

                if (wamid == null)
                    return NotFound();

                var messageDto = new WhatsappIncomingMessageDto
                {
                    MessageId = wamid,
                    From = dto.PhoneNumber,
                    Text = dto.Message,
                    Type = "text",
                    ReplyToMessageId = wamid,
                    Direction = "OUT",
                    Status = "sent"
                };

                var response = await _messageUnitOfWork
                .AddReceiveMessages(messageDto);

                if (!response.Success)
                    return BadRequest("No se pudo enviar el mensaje");

                await _hub.Clients.All.SendAsync(
               "InboxUpdated",
                    new InboxConversationDto
                    {
                        PhoneNumber = dto.PhoneNumber,
                        LastMessage = dto.Message,
                        LastMessageAt = DateTime.UtcNow,
                        UnreadCount = 1
                    }
                );

                // 6️⃣ Enviar mensaje SOLO al chat abierto
                await _hub.Clients.Group(dto.PhoneNumber).SendAsync(
                    "NewMessage",
                    new RealtimeChatMessageDto
                    {
                        PhoneNumber = dto.PhoneNumber,
                        Direction = "OUT",
                        MessageType = "text",
                        Content = dto.Message,
                        Timestamp = DateTime.UtcNow
                    }
                );
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}