using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Security.Claims;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/WhatsAppOnboarding")]
public class WhatsAppOnboardingController : ControllerBase
{
    private readonly MetaAuthService _metaAuth;
    private readonly IUsuarioWhatsAppConfigUnitOfWork _repo;

    public WhatsAppOnboardingController(MetaAuthService metaAuth, IUsuarioWhatsAppConfigUnitOfWork repo)
    {
        _metaAuth = metaAuth;
        _repo = repo;
    }

    [HttpPost("exchange-code")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ExchangeCode(
    [FromBody] ExchangeCodeRequest request)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (usuarioId == null)
            return Unauthorized();

        var existing = await _repo.GetByPhoneNumberIdAsync(request.PhoneNumberId);
        if (existing != null)
            return Conflict("Este número ya está configurado");

        try
        {
            var tempToken = await _metaAuth.ExchangeCodeAsync(request.Code);

            var systemUserId = await _metaAuth.CreateSystemUserAsync(
                request.BusinessId,
                tempToken.AccessToken
            );

            var permanentToken = await _metaAuth.GeneratePermanentTokenAsync(
                systemUserId,
                tempToken.AccessToken
            );

            var config = new UsuarioWhatsAppConfig
            {
                UsuarioId = usuarioId,
                BusinessId = request.BusinessId,
                WabaId = request.WabaId,
                PhoneNumberId = request.PhoneNumberId,
                SystemUserId = systemUserId,
                AccessToken = permanentToken
            };

            await _repo.AddAsync(config);

            return Ok(new
            {
                success = true,
                configId = config.Id
            });
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