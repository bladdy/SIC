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
using SIC.Shared.Entities;

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

        //ToDo:Agregar una tabla para ver si se envio o no la invitacion, para evitar enviar varias veces la misma invitacion a un mismo numero, y agregar un campo de fecha de envio para llevar un control de cuando se envio la invitacion
        [HttpPost("enviar-invitacion")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,WeddingPlanner,User")]
        public async Task<IActionResult> EnviarInvitacionMasiva(
            [FromBody] MasiveSendTemplateDTO sendTemplateDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });
            var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
            if (!userWhatsAppConfig.Success)
                return BadRequest(new { error = "Este usuario no tiene WhatsApp configurado" });

            var accessToken = userWhatsAppConfig.Result!.AccessToken;
            var phoneNumberId = userWhatsAppConfig.Result!.PhoneNumberId;
            var templateName = sendTemplateDTO.TemplateName;

            int enviados = 0;
            int fallidos = 0;

            var errores = new List<object>();

            foreach (var code in sendTemplateDTO.Codes)
            {
                try
                {
                    var invitacion = await _invitationUnitOfWork.GetByCodeAsync(code);
                    if (invitacion.Result == null)
                    {
                        fallidos++;
                        errores.Add(new { Code = code, Error = "Invitación no encontrada" });
                        continue;
                    }

                    var ev = invitacion.Result.Event!;
                    var fechaFormateada = FechaHelper.FormatearFechaLargaEspanol(ev.Date);

                    string coverImageUrl = !string.IsNullOrWhiteSpace(ev.CoverImageUrl)
                        ? ev.CoverImageUrl
                        : "https://invboxv-app.com/logo.png";

                    var parametros = new List<string>
                    {
                        invitacion.Result.Name,
                        ev.Name,
                        ev.SubTitle,
                        $"{ev.Url}?codigo={code}",
                        fechaFormateada,
                        ev.Name
                    };

                    var result = await _whatsAppService.EnviarInvitacionAsync(
                        accessToken,
                        phoneNumberId,
                        invitacion.Result.PhoneNumber,
                        templateName,
                        "es_ES",
                        coverImageUrl,
                        parametros
                    );

                    if (!result.Success)
                    {
                        fallidos++;
                        errores.Add(new { Code = code, Error = result.Message });
                        continue;
                    }

                    enviados++;

                    var messageDto = new WhatsappIncomingMessageDto
                    {
                        PhoneNumber = userWhatsAppConfig.Result.PhoneNumber,
                        MessageId = result.Result!.Wamid,
                        From = invitacion.Result.PhoneNumber,
                        Text = $"Invitación enviada a {invitacion.Result.Name}",
                        Type = "template",
                        Direction = "OUT",
                        Status = "sent",
                        Timestamp = DateTime.UtcNow
                    };

                    await _iMessageUnitOfWork.AddReceiveMessages(messageDto);
                    await SaveMessageHistory(invitacion.Result.Code!, result);

                    // ⏱️ DELAY ANTI BLOQUEO (RECOMENDADO)
                    await Task.Delay(1200); // 1.2 segundos
                }
                catch (Exception ex)
                {
                    fallidos++;
                    errores.Add(new { Code = code, Error = ex.Message });
                }
            }

            return Ok(new
            {
                success = true,
                enviados,
                fallidos,
                total = sendTemplateDTO.Codes.Count,
                errores
            });
        }

        [HttpPost("enviar-invitacion/{code}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,WeddingPlanner,User")]
        public async Task<IActionResult> EnviarInvitacion(string code)
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });
            var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
            if (userWhatsAppConfig.Result == null)
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
                PhoneNumber = userWhatsAppConfig.Result.PhoneNumber,
                MessageId = result.Result!.Wamid,
                From = result.Result!.Contact,
                Text = $"Invitacion enviada a {invitacion.Result.Name}, con la url {invitacion.Result.Event!.Url!}?codigo={code}",
                Type = "template",
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

        //_whatsAppConfigUnitOfWork
        [HttpGet("configurar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,WeddingPlanner,User")]
        public async Task<IActionResult> ConfigurarWhatsApp()
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });

            var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
            if (userWhatsAppConfig.Success)
            {
                return Ok(userWhatsAppConfig.Result);
            }
            return NotFound(userWhatsAppConfig.Message);
        }

        //_whatsAppConfigUnitOfWork
        [HttpPost("configurar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,WeddingPlanner,User")]
        public async Task<IActionResult> ConfigurarWhatsApp([FromBody] WhatsAppManualConfigDto usuarioWhatsAppConfig)
        {
            // Extraer el ID del usuario autenticado desde el token JWT
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });
            var newUsuarioWhatsAppConfig = new UsuarioWhatsAppConfig
            {
                AccessToken = usuarioWhatsAppConfig.AccessToken,
                PhoneNumberId = usuarioWhatsAppConfig.PhoneNumberId,
                WabaId = usuarioWhatsAppConfig.WabaId,
                SystemUserId = usuarioWhatsAppConfig.SystemUserId,
                BusinessId = usuarioWhatsAppConfig.BusinessId,
                PhoneNumber = usuarioWhatsAppConfig.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                UsuarioId = userId,
            };

            var action = await _whatsAppConfigUnitOfWork.AddFullAsync(newUsuarioWhatsAppConfig);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }

        //_whatsAppConfigUnitOfWork
        [HttpPut("configurar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,WeddingPlanner,User")]
        public async Task<IActionResult> ConfigurarWhatsAppPut([FromBody] WhatsAppManualConfigDto usuarioWhatsAppConfig)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });
            var newUsuarioWhatsAppConfig = new UsuarioWhatsAppConfig
            {
                AccessToken = usuarioWhatsAppConfig.AccessToken,
                PhoneNumberId = usuarioWhatsAppConfig.PhoneNumberId,
                WabaId = usuarioWhatsAppConfig.WabaId,
                SystemUserId = usuarioWhatsAppConfig.SystemUserId,
                BusinessId = usuarioWhatsAppConfig.BusinessId,
                PhoneNumber = usuarioWhatsAppConfig.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                UsuarioId = userId,
            };
            var action = await _whatsAppConfigUnitOfWork.UpdateFullAsync(newUsuarioWhatsAppConfig);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
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