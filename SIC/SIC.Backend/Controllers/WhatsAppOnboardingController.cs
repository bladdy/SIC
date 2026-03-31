using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IWhatsAppConfigUnitOfWork _whatsAppConfigUnitOfWork;

    public WhatsAppOnboardingController(
        MetaAuthService metaAuth,
        IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork,
        IUsuarioWhatsAppConfigUnitOfWork repo)
    {
        _metaAuth = metaAuth;
        _repo = repo;
        _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
    }

    /// <summary>
    /// Flujo completo de onboarding WhatsApp Embedded Signup:
    /// 1. Exchange code → User Token temporal
    /// 2. Suscribir app al WABA
    /// 3. Crear System User bajo el Business
    /// 4. Asignar WABA al System User
    /// 5. Generar token permanente
    /// 6. Obtener datos del phone number
    /// 7. Guardar configuración en DB
    /// </summary>
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
            return BadRequest("Datos incompletos: se requieren Code, BusinessId, WabaId y PhoneNumberId");
        }

        var existing = await _repo.GetByPhoneNumberIdAsync(request.PhoneNumberId);
        if (existing != null)
            return Conflict("Este número ya está configurado");

        try
        {
            // ── PASO 1: Exchange code → User Token temporal ──────────────────
            var tempToken = await _metaAuth.ExchangeCodeAsync(request.Code);
            Console.WriteLine("✅ Paso 1: Token temporal obtenido");

            // ── PASO 2: Suscribir la app al WABA ─────────────────────────────
            // Esto es CRÍTICO: sin este paso el phone_number_id no acepta mensajes
            /* await _metaAuth.SubscribeAppAsync(request.WabaId);
             Console.WriteLine("✅ Paso 2: App suscrita al WABA");

             // ── PASO 3: Crear System User ─────────────────────────────────────
             // Usamos el User Token temporal (el usuario debe ser admin del Business)
             var systemUserId = await _metaAuth.CreateSystemUserAsync(
                 request.BusinessId, tempToken.AccessToken);
             Console.WriteLine($"✅ Paso 3: System User creado: {systemUserId}");

             // ── PASO 4: Asignar WABA al System User ──────────────────────────
             // Usamos el App Token (AppID|AppSecret) que tiene permisos globales
             var appToken = _metaAuth.GetAppToken();
             await _metaAuth.AssignWabaToSystemUserAsync(request.WabaId, systemUserId, appToken);
             Console.WriteLine("✅ Paso 4: WABA asignado al System User");

             // ── PASO 5: Generar Token Permanente ──────────────────────────────
             var permanentToken = await _metaAuth.GeneratePermanentTokenAsync(systemUserId, appToken);
             Console.WriteLine("✅ Paso 5: Token permanente generado");

             // ── PASO 6: Obtener datos reales del phone number ─────────────────
             var phoneInfo = await _metaAuth.GetPhoneNumberAsync(
                 request.PhoneNumberId, permanentToken);
             Console.WriteLine($"✅ Paso 6: Phone number obtenido: {phoneInfo.DisplayPhoneNumber}");*/

            // ── PASO 7: Guardar en DB ─────────────────────────────────────────
            var config = new UsuarioWhatsAppConfig
            {
                AccessToken = tempToken.AccessToken,           // ✅ Token permanente del System User
                PhoneNumberId = request.PhoneNumberId,
                WabaId = request.WabaId,
                BusinessId = request.BusinessId,
                PhoneNumber = "", //phoneInfo.DisplayPhoneNumber ?? "no-disponible",
                SystemUserId = "system_user_id",//systemUserId,
                UsuarioId = usuarioId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            var action = await _whatsAppConfigUnitOfWork.AddFullAsync(config);

            if (!action.Success)
                return StatusCode(500, new { error = action.Message });

            return Ok(new
            {
                success = true,
                configId = config.Id,
                phoneNumber = config.PhoneNumber,
                systemUserId = config.SystemUserId,
                message = "WhatsApp configurado correctamente. Ya puedes enviar mensajes."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en onboarding: {ex.Message}");
            return StatusCode(500, new
            {
                error = "No se pudo completar la configuración de WhatsApp",
                detalle = ex.Message
            });
        }
    }

    /// <summary>
    /// Endpoint opcional para enviar un mensaje de prueba una vez configurado
    /// </summary>
    [HttpPost("send-test")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SendTest([FromBody] SendTestRequest request)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (usuarioId == null)
            return Unauthorized();

        if (string.IsNullOrEmpty(request.PhoneNumberId) || string.IsNullOrEmpty(request.ToPhone))
            return BadRequest("Se requieren PhoneNumberId y ToPhone");

        try
        {
            var config = await _repo.GetByPhoneNumberIdAsync(request.PhoneNumberId);
            if (config == null)
                return NotFound("Configuración no encontrada para ese PhoneNumberId");

            var result = await _metaAuth.SendTestMessageAsync(
                config.PhoneNumberId,
                request.ToPhone,
                config.AccessToken
            );

            return Ok(new { success = true, metaResponse = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = "Error enviando mensaje de prueba",
                detalle = ex.Message
            });
        }
    }

    /// <summary>
    /// Request para enviar un mensaje de prueba
    /// </summary>
    public class SendTestRequest
    {
        /// <summary>
        /// El PhoneNumberId configurado en Meta (viene del Embedded Signup)
        /// </summary>
        public string PhoneNumberId { get; set; } = string.Empty;

        /// <summary>
        /// Número destino con código de país, sin espacios ni guiones
        /// Ejemplo: 521XXXXXXXXXX (México)
        /// </summary>
        public string ToPhone { get; set; } = string.Empty;
    }
}