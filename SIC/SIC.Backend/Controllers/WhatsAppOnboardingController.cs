using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Security.Claims;
using static QRCoder.PayloadGenerator;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/WhatsAppOnboarding")]
public class WhatsAppOnboardingController : ControllerBase
{
    private readonly MetaAuthService _metaAuth;
    private readonly IUsuarioWhatsAppConfigUnitOfWork _repo;
    private readonly IWhatsAppConfigUnitOfWork _whatsAppConfigUnitOfWork;

    public WhatsAppOnboardingController(MetaAuthService metaAuth,
            IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork, IUsuarioWhatsAppConfigUnitOfWork repo)
    {
        _metaAuth = metaAuth;
        _repo = repo;
        _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
    }

    [HttpPost("exchange-code")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ExchangeCode([FromBody] ExchangeCodeRequest request)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (usuarioId == null)
            return Unauthorized();

        if (string.IsNullOrEmpty(request.Code) ||
            string.IsNullOrEmpty(request.BusinessId) ||
            string.IsNullOrEmpty(request.WabaId) ||
            string.IsNullOrEmpty(request.PhoneNumberId))
        {
            return BadRequest("Datos incompletos");
        }

        var existing = await _repo.GetByPhoneNumberIdAsync(request.PhoneNumberId);
        if (existing != null)
            return Conflict("Este número ya está configurado");

        try
        {
            var tempToken = await _metaAuth.ExchangeCodeAsync(request.Code);
            /*
            var systemUserId = await _metaAuth.CreateSystemUserAsync(
                request.BusinessId,
                tempToken.AccessToken
            );*/
            /*
            var permanentToken = await _metaAuth.GeneratePermanentTokenAsync(
                systemUserId,
                tempToken.AccessToken
            );*/
            // 🔥 SUSCRIBIR EL WABA AL WEBHOOK (AQUÍ)
            /*
            await _metaAuth.SubscribeAppAsync(
                request.WabaId,
                tempToken.AccessToken//permanentToken
            );*/
            // 🔥 Obtener número real
            /*var phoneNumber = await _metaAuth.GetPhoneNumberAsync(
                request.PhoneNumberId,
                tempToken.AccessToken//permanentToken
            );*/

            /*var phoneNumber = await _metaAuth.GetPhoneNumbersFromWaba(
                request.WabaId,
                tempToken.AccessToken//permanentToken
            );*/

            //await SendTestMessage(token.AccessToken, phoneNumberId);

            var config = new UsuarioWhatsAppConfig
            {
                AccessToken = tempToken.AccessToken,//permanentToken, // ⚠️ encriptar en producción
                PhoneNumberId = request.PhoneNumberId,
                WabaId = request.WabaId,
                BusinessId = request.BusinessId,
                PhoneNumber = "nodisponible",//phoneNumber.DisplayPhoneNumber,
                SystemUserId = "SIC WhatsApp System User", //systemUserId,
                UsuarioId = usuarioId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };
            //Guardar la configuracion del usuario en la base de datos
            var action = await _whatsAppConfigUnitOfWork.AddFullAsync(config);
            if (action.Success)
            {
                return Ok(new
                {
                    success = true,
                    configId = config.Id,
                    phoneNumber = config.PhoneNumber
                });
            }

            return NotFound(action.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "No se pudo completar la configuración de WhatsApp",
                metaError = ex.Message
            });
        }
    }
}