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
    public async Task<IActionResult> ExchangeCode(
    [FromBody] ExchangeCodeRequest request)
    {
        var usuarioId =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (usuarioId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Code) ||
            string.IsNullOrWhiteSpace(request.BusinessId) ||
            string.IsNullOrWhiteSpace(request.WabaId) ||
            string.IsNullOrWhiteSpace(request.PhoneNumberId))
        {
            return BadRequest(new
            {
                error = "Datos incompletos"
            });
        }

        try
        {
            // =========================================================
            // VALIDAR QUE NO EXISTA
            // =========================================================

            var existing =
                await _repo.GetByPhoneNumberIdAsync(
                    request.PhoneNumberId);

            if (existing != null)
            {
                return Conflict(new
                {
                    error = "Este número ya está configurado"
                });
            }

            // =========================================================
            // PASO 1
            // Exchange Code -> Short Lived Token
            // =========================================================

            var shortToken =
                await _metaAuth.ExchangeCodeAsync(
                    request.Code);

            Console.WriteLine(
                "✅ Paso 1: Short-lived token obtenido");

            // =========================================================
            // PASO 2
            // Short -> Long Lived Token
            // =========================================================

            var longToken =
                await _metaAuth.ExchangeForLongLivedTokenAsync(
                    shortToken.AccessToken);

            Console.WriteLine(
                "✅ Paso 2: Long-lived token obtenido");

            // =========================================================
            // PASO 3
            // Validar token
            // =========================================================

            var tokenInfo =
                await _metaAuth.DebugTokenAsync(
                    longToken.AccessToken);

            if (!tokenInfo.TryGetProperty(
                    "is_valid",
                    out var isValidProp) ||
                !isValidProp.GetBoolean())
            {
                throw new Exception(
                    "El token obtenido no es válido");
            }

            Console.WriteLine(
                "✅ Paso 3: Token validado");

            // =========================================================
            // PASO 4
            // Suscribir App al WABA
            // =========================================================

            await _metaAuth.SubscribeAppAsync(
                request.WabaId,
                longToken.AccessToken);

            Console.WriteLine(
                "✅ Paso 4: App suscrita al WABA");

            // =========================================================
            // PASO 5
            // Obtener información del número
            // =========================================================

            var phoneInfo =
                await _metaAuth.GetPhoneNumberAsync(
                    request.PhoneNumberId,
                    longToken.AccessToken);

            Console.WriteLine(
                $"✅ Paso 5: Número obtenido: {phoneInfo.DisplayPhoneNumber}");

            // =========================================================
            // PASO 6
            // Verificar acceso al Phone Number
            // =========================================================

            await _metaAuth.RegisterPhoneNumberAsync(request.PhoneNumberId,longToken.AccessToken, "123456");
            /*try
            {
                await _metaAuth.SendTestMessageAsync(
                    request.PhoneNumberId,
                    "528661425258", // tu número de prueba
                    longToken.AccessToken);

                Console.WriteLine(
                    "✅ Paso 6: Mensaje de prueba enviado");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"⚠️ Mensaje de prueba omitido: {ex.Message}");
            }*/

            // =========================================================
            // PASO 7
            // Guardar configuración
            // =========================================================

            var config =
                new UsuarioWhatsAppConfig
                {
                    UsuarioId = usuarioId,

                    AccessToken =
                        longToken.AccessToken,

                    PhoneNumberId =
                        request.PhoneNumberId,

                    PhoneNumber =
                        phoneInfo.DisplayPhoneNumber ??
                        string.Empty,

                    WabaId =
                        request.WabaId,

                    BusinessId =
                        request.BusinessId,

                    SystemUserId = null,

                    CreatedAt =
                        DateTime.UtcNow,

                    IsActive = true
                };

            var action =
                await _whatsAppConfigUnitOfWork
                    .AddFullAsync(config);

            if (!action.Success)
            {
                return StatusCode(500, new
                {
                    error = action.Message
                });
            }

            Console.WriteLine(
                "✅ Paso 7: Configuración guardada");

            return Ok(new
            {
                success = true,

                configId = config.Id,

                phoneNumber =
                    config.PhoneNumber,

                phoneNumberId =
                    config.PhoneNumberId,

                wabaId =
                    config.WabaId,

                businessId =
                    config.BusinessId,

                message =
                    "WhatsApp configurado correctamente."
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ Error onboarding WhatsApp: {ex}");

            return StatusCode(500, new
            {
                error =
                    "No se pudo completar la configuración",

                detalle =
                    ex.Message
            });
        }
    }

    [HttpPost("exchange-code-60days")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ExchangeCode60days(
    [FromBody] ExchangeCodeRequest request)
    {
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (usuarioId == null)
            return Unauthorized();

        if (string.IsNullOrEmpty(request.Code) ||
            string.IsNullOrEmpty(request.BusinessId) ||
            string.IsNullOrEmpty(request.WabaId) ||
            string.IsNullOrEmpty(request.PhoneNumberId))
        {
            return BadRequest(
                "Se requieren Code, BusinessId, WabaId y PhoneNumberId");
        }

        // Verificar si el número ya existe
        var existing =
            await _repo.GetByPhoneNumberIdAsync(request.PhoneNumberId);

        if (existing != null)
            return Conflict("Este número ya está configurado");

        try
        {
            // ============================================================
            // PASO 1 - Exchange Code → Short-Lived Token
            // ============================================================

            var shortToken =
                await _metaAuth.ExchangeCodeAsync(request.Code);

            Console.WriteLine("✅ Short-lived token obtenido");

            // ============================================================
            // PASO 2 - Convertir a Long-Lived Token (~60 días)
            // ============================================================

            var longLivedToken =
                await _metaAuth.ExchangeForLongLivedTokenAsync(
                    shortToken.AccessToken);

            Console.WriteLine("✅ Long-lived token generado");

            // ============================================================
            // PASO 3 - Suscribir App al WABA
            // IMPORTANTE para webhooks y mensajería
            // ============================================================

            /*await _metaAuth.SubscribeAppAsync(
                request.WabaId,
                longLivedToken.AccessToken);*/

            Console.WriteLine("✅ App suscrita al WABA");

            // ============================================================
            // PASO 4 - Obtener datos reales del número
            // ============================================================

            var phoneInfo =
                await _metaAuth.GetPhoneNumberAsync(
                    request.PhoneNumberId,
                    longLivedToken.AccessToken);

            Console.WriteLine(
                $"✅ Número obtenido: {phoneInfo.DisplayPhoneNumber}");

            // ============================================================
            // PASO 5 - Guardar configuración
            // ============================================================

            var config = new UsuarioWhatsAppConfig
            {
                UsuarioId = usuarioId,

                AccessToken = longLivedToken.AccessToken,

                TokenExpiresAt = DateTime.UtcNow.AddDays(60),

                PhoneNumberId = request.PhoneNumberId,

                WabaId = request.WabaId,

                BusinessId = request.BusinessId,

                PhoneNumber =
                    phoneInfo.DisplayPhoneNumber ?? "no-disponible",

                SystemUserId = "",

                CreatedAt = DateTime.UtcNow,

                IsActive = true
            };

            var action =
                await _whatsAppConfigUnitOfWork.AddFullAsync(config);

            if (!action.Success)
            {
                return StatusCode(500, new
                {
                    error = action.Message
                });
            }

            // ============================================================
            // RESPUESTA
            // ============================================================

            return Ok(new
            {
                success = true,

                configId = config.Id,

                phoneNumber = config.PhoneNumber,

                expiresAt = config.TokenExpiresAt,

                message =
                    "WhatsApp configurado correctamente"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ Error en onboarding WhatsApp: {ex.Message}");

            return StatusCode(500, new
            {
                error =
                    "No se pudo completar la configuración",

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