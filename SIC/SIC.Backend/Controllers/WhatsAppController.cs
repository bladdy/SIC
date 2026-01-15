using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.DTOs;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Helpers;
using SIC.Shared.Response;
using SIC.Shared.DTOs;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly WhatsAppService _whatsAppService;
        private readonly IConfiguration _configuration;
        private readonly IInvitationUnitOfWork _invitationUnitOfWork;
        private readonly IMessageUnitOfWork _iMessageUnitOfWork;

        public WhatsAppController(
            WhatsAppService whatsAppService, IInvitationUnitOfWork invitationUnitOfWork,
            IConfiguration configuration, IMessageUnitOfWork iMessageUnitOfWork)
        {
            _whatsAppService = whatsAppService;
            _configuration = configuration;
            _invitationUnitOfWork = invitationUnitOfWork;
            _iMessageUnitOfWork = iMessageUnitOfWork;
        }

        [HttpPost("enviar-invitacion/{code}")]
        public async Task<IActionResult> EnviarInvitacion(string code)
        {
            var accessToken = _configuration["WhatsApp:AccessToken"];
            var phoneNumberId = _configuration["WhatsApp:PhoneNumberId"];
            List<string> parametros = new List<string>();
            var invitacion = await _invitationUnitOfWork.GetByCodeAsync(code);
            if (invitacion.Result == null)
                return NotFound(new { error = "Invitación no encontrada." });

            var fecha = invitacion.Result.Event!.Date;

            string fechaFormateada = FechaHelper.FormatearFechaLargaEspanol(fecha);

            parametros.Add(invitacion.Result.Name);
            parametros.Add(invitacion.Result.Event!.Name);
            parametros.Add(invitacion.Result.Event!.SubTitle);
            parametros.Add($"{invitacion.Result.Event!.Url!}{invitacion.Result.Code}");
            parametros.Add(fechaFormateada);
            parametros.Add(invitacion.Result.Event!.Name);

            var result = await _whatsAppService.EnviarInvitacionAsync(
                accessToken!,
                phoneNumberId!,
                invitacion.Result.PhoneNumber,
                "Confirmacion",
                "Es-MX",
                parametros
            );
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