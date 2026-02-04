using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.DTOs;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Helpers;
using SIC.Shared.Response;
using SIC.Shared.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly WhatsAppService _whatsAppService;
        private readonly IWhatsAppConfigUnitOfWork _whatsAppConfigUnitOfWork;
        private readonly IInvitationUnitOfWork _invitationUnitOfWork;
        private readonly IMessageUnitOfWork _iMessageUnitOfWork;

        public WhatsAppController(
            WhatsAppService whatsAppService, IInvitationUnitOfWork invitationUnitOfWork,
            IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork, IMessageUnitOfWork iMessageUnitOfWork)
        {
            _whatsAppService = whatsAppService;
            _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
            _invitationUnitOfWork = invitationUnitOfWork;
            _iMessageUnitOfWork = iMessageUnitOfWork;
        }

        [HttpPost("enviar-invitacion/{code}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,WeddingPlanner,User")]
        public async Task<IActionResult> EnviarInvitacion(string code)
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
            if (!userWhatsAppConfig.Success)
                return BadRequest(new { error = "Este usuario no tiene permisos para hacer este envio" });

            //obtener los datos del usuario
            var accessToken = userWhatsAppConfig.Result!.AccessToken;
            var phoneNumberId = userWhatsAppConfig.Result!.PhoneNumberId;
            List<string> parametros = new List<string>();
            var invitacion = await _invitationUnitOfWork.GetByCodeAsync(code);
            if (invitacion.Result == null)
                return NotFound(new { error = "Invitación no encontrada." });

            var fecha = invitacion.Result.Event!.Date;
            string coverImageUrl =
                    !string.IsNullOrWhiteSpace(invitacion.Result.Event?.CoverImageUrl)
                            ? invitacion.Result.Event.CoverImageUrl
                            : "https://invboxv-app.com/logo.png";

            string fechaFormateada = FechaHelper.FormatearFechaLargaEspanol(fecha);

            parametros.Add(invitacion.Result.Name);
            parametros.Add(invitacion.Result.Event!.Name);
            parametros.Add(invitacion.Result.Event!.SubTitle);
            parametros.Add($"{invitacion.Result.Event!.Url!}?codigo={code}");
            parametros.Add(fechaFormateada);
            parametros.Add(invitacion.Result.Event!.Name);

            var result = await _whatsAppService.EnviarInvitacionAsync(
                accessToken!,
                phoneNumberId!,
                invitacion.Result.PhoneNumber,
                "confirmaciones",
                "es_Es",
                coverImageUrl,
                parametros

            );
            var messageDto = new WhatsappIncomingMessageDto
            {
                MessageId = result.Message,
                From = result.Result!.Contact,
                Text = $"Invitacion enviada a {invitacion.Result.Name}, con la url {invitacion.Result.Event!.Url!}?codigo={code}",
                Type = "text",
                ReplyToMessageId = result.Result!.Wamid,
                Direction = "OUT",
                Status = "sent"
            };

            var response = await _iMessageUnitOfWork
            .AddReceiveMessages(messageDto);

            if (!response.Success)
                return BadRequest("No se pudo enviar el mensaje");

            await SaveMessageHistory(invitacion.Result.Code!, result);

            if (!result.Success)
                return BadRequest(new { error = result });

            return Ok(result.Result);
        }

        private async Task<ActionResponse<bool>> SaveMessageHistory(string code, ActionResponse<WhatsAppMessageResponse> result)
        {
            return await _iMessageUnitOfWork.AddHistoryMessages(
                code,
                result.Success,
                result.Success ? "Mensaje enviado correctamente." : result.Message
            );
        }
    }
}