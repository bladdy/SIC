using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Helpers;
using SIC.Shared.Response;
using SIC.Shared.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using SIC.Shared.Entities;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.Request;
using System.Text.Json;
using SIC.Backend.Helpers;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SIC.Backend.UnitOfWork.Implemetations;

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
        private readonly IWhatsAppTemplateBuilderService _templateBuilderService;
        private readonly IWhatsAppTemplateRepository _templateRepository;

        public WhatsAppController(
            WhatsAppService whatsAppService, IInvitationUnitOfWork invitationUnitOfWork, IWhatsAppTemplateBuilderService templateBuilderService, IWhatsAppTemplateRepository templateRepository,
            IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork, IMessageUnitOfWork iMessageUnitOfWork)
        {
            _whatsAppService = whatsAppService;
            _templateBuilderService = templateBuilderService;
            _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
            _invitationUnitOfWork = invitationUnitOfWork;
            _iMessageUnitOfWork = iMessageUnitOfWork;
            _templateRepository = templateRepository;
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
                        invitacion.Result.PhoneNumber!,
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
                        From = invitacion.Result.PhoneNumber!,
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

        [HttpGet("get-templates")]
        public async Task<IActionResult> GetTemplates()
        {
            var templates = await _templateRepository.GetAllAsync();
            return Ok(templates.Result);
        }

        [HttpPost("create-templates")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateModel model)
        {/*
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });

            var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
            if (!userWhatsAppConfig.Success)
                return BadRequest(new { error = "Este usuario no tiene WhatsApp configurado" });*/

            var accessToken = "EAAUu8FHu8ZAwBQsU6ZAoXcUw8GZClIsc7h1JGvgz0ZBQdWO7XxIMkafA1TzRls0Jn1ZCMeQRkg95Cj4PeUkuCAR3YvpzpWCuEr3HmdL3D5w0meDRVqwZC5vE9O4fK1MVnrHT64UsfQQb25BWATpEm2nEa822WdceVjoFe9lHYlcxlb4BkGpyYK3uEIoZCzkfggiMgZDZD";//userWhatsAppConfig.Result!.AccessToken;
            var wabaId = "1265194188987559";//userWhatsAppConfig.Result.WabaId;
            var request = BuildWhatsappTemplateJson(model);

            var result = await _whatsAppService.CreateWhatsAppTemplateAsync(accessToken, wabaId, request, model.MediaUrl);

            return result ? Ok() : StatusCode(500, new { error = "Error al crear plantilla en WhatsApp" });
        }

        private string BuildWhatsappTemplateJson(CreateTemplateModel model)
        {
            if (model.Components == null || !model.Components.Any())
                throw new Exception("El template debe tener al menos un componente");

            var componentsList = new List<Dictionary<string, object>>();

            // =========================
            // HEADER
            // =========================
            if (!string.IsNullOrEmpty(model.MediaUrl))
            {
                componentsList.Add(new Dictionary<string, object>
                {
                    ["type"] = "HEADER",
                    ["format"] = model.MediaType?.ToUpper(),
                    ["example"] = new
                    {
                        header_handle = new List<string> { model.MediaUrl }
                    }
                });
            }
            else if (model.Header != null && !string.IsNullOrWhiteSpace(model.Header.Text))
            {
                componentsList.Add(new Dictionary<string, object>
                {
                    ["type"] = "HEADER",
                    ["format"] = "TEXT",
                    ["text"] = model.Header.Text
                });
            }

            // =========================
            // BODY
            // =========================
            var bodyComponent = model.Components
                .FirstOrDefault(c => c.Type.Equals("body", StringComparison.OrdinalIgnoreCase));

            if (bodyComponent == null || string.IsNullOrWhiteSpace(bodyComponent.Text))
                throw new Exception("El componente BODY debe tener texto");

            var bodyComp = new Dictionary<string, object>
            {
                ["type"] = "BODY",
                ["text"] = bodyComponent.Text
            };

            // Detectar variables {{param}}
            var matches = Regex.Matches(bodyComponent.Text, "{{(.*?)}}");

            if (matches.Count > 0)
            {
                var namedParams = matches
                    .Select(m => m.Groups[1].Value.Trim())
                    .Distinct()
                    .Select(p => new
                    {
                        param_name = p,
                        example = "Ejemplo"
                    })
                    .ToList();

                bodyComp["example"] = new
                {
                    body_text_named_params = namedParams
                };
            }

            componentsList.Add(bodyComp);

            // =========================
            // FOOTER
            // =========================
            if (!string.IsNullOrWhiteSpace(model.Footer))
            {
                componentsList.Add(new Dictionary<string, object>
                {
                    ["type"] = "FOOTER",
                    ["text"] = model.Footer
                });
            }

            // =========================
            // BUTTONS
            // =========================
            var buttonsList = new List<ButtonRequest>();

            var buttonsComponent = model.Components
                .FirstOrDefault(c => c.Type.Equals("buttons", StringComparison.OrdinalIgnoreCase));

            if (buttonsComponent?.Buttons != null)
                buttonsList.AddRange(buttonsComponent.Buttons);

            if (model.Buttons != null)
                buttonsList.AddRange(model.Buttons.Select(b => new ButtonRequest
                {
                    Type = b.Type,
                    Text = b.Text,
                    Url = b.Url
                }));

            if (buttonsList.Any())
            {
                var formattedButtons = new List<Dictionary<string, object>>();

                foreach (var b in buttonsList)
                {
                    var typeUpper = b.Type?.ToUpper();

                    var btn = new Dictionary<string, object>
                    {
                        ["type"] = typeUpper,
                        ["text"] = b.Text
                    };

                    if (typeUpper == "URL" && !string.IsNullOrWhiteSpace(b.Url))
                    {
                        btn["url"] = b.Url;

                        // Si la URL tiene {{param}}, es dinámica → necesita example
                        if (b.Url.Contains("{{"))
                        {
                            var match = Regex.Match(b.Url, "{{(.*?)}}");
                            if (match.Success)
                            {
                                btn["example"] = new List<string> { "EJEMPLO" };
                            }
                        }
                    }

                    if (typeUpper == "PHONE_NUMBER" && !string.IsNullOrWhiteSpace(b.Url))
                    {
                        btn["phone_number"] = b.Url;
                    }

                    formattedButtons.Add(btn);
                }

                componentsList.Add(new Dictionary<string, object>
                {
                    ["type"] = "BUTTONS",
                    ["buttons"] = formattedButtons
                });
            }

            // =========================
            // FINAL OBJECT
            // =========================
            var finalObj = new
            {
                name = NormalizeStrings.NormalizeTemplateName(model.Name),
                language = model.Language,
                category = model.Category,
                parameter_format = "NAMED",
                components = componentsList
            };

            return JsonSerializer.Serialize(finalObj, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }

        /*
        private string BuildWhatsappTemplateJson(CreateTemplateModel model)
        {
            if (model.Components == null || !model.Components.Any())
                throw new Exception("El template debe tener al menos un componente");

            var componentsList = new List<Dictionary<string, object>>();

            // HEADER (imagen o texto)
            if (!string.IsNullOrEmpty(model.MediaUrl))
            {
                componentsList.Add(new Dictionary<string, object>
                {
                    ["type"] = "header",
                    ["format"] = model.MediaType.ToLower(), // image, video, document
                    ["example"] = new { header_handle = new List<string> { model.MediaUrl } }
                });
            }
            else if (model.Header != null && !string.IsNullOrEmpty(model.Header.Text))
            {
                componentsList.Add(new Dictionary<string, object>
                {
                    ["type"] = "header",
                    ["format"] = "text",
                    ["text"] = model.Header.Text
                });
            }

            // BODY
            var bodyComponent = model.Components.FirstOrDefault(c => c.Type.ToLower() == "body");
            if (bodyComponent == null || string.IsNullOrWhiteSpace(bodyComponent.Text))
                throw new Exception("El componente BODY debe tener texto");

            var bodyComp = new Dictionary<string, object>
            {
                ["type"] = "body",
                ["text"] = bodyComponent.Text
            };

            // CREAR body_text_named_params basado en {{param}}
            var matches = Regex.Matches(bodyComponent.Text, "{{(.*?)}}");
            if (matches.Count > 0)
            {
                var namedParams = matches
                    .Select(m => new
                    {
                        param_name = m.Groups[1].Value.Trim(),
                        example = "Ejemplo"
                    })
                    .DistinctBy(p => p.param_name)
                    .ToList();

                bodyComp["example"] = new { body_text_named_params = namedParams };
            }

            componentsList.Add(bodyComp);

            // FOOTER
            if (!string.IsNullOrWhiteSpace(model.Footer))
            {
                componentsList.Add(new Dictionary<string, object>
                {
                    ["type"] = "footer",
                    ["text"] = model.Footer
                });
            }

            // BUTTONS (desde el componente o desde la lista general)
            List<ButtonRequest> buttonsList = new List<ButtonRequest>();
            var buttonsComponent = model.Components.FirstOrDefault(c => c.Type.ToLower() == "buttons");
            if (buttonsComponent?.Buttons != null)
                buttonsList.AddRange(buttonsComponent.Buttons);

            if (model.Buttons != null)
                buttonsList.AddRange(model.Buttons.Select(b => new ButtonRequest
                {
                    Type = b.Type,
                    Text = b.Text,
                    Url = b.Url,
                    UrlType = b.UrlType,
                    UrlBase = b.UrlBase,
                    DynamicExample = b.DynamicExample,
                    //Example = b.Example
                }));

            if (buttonsList.Any())
            {
                var buttonsComp = new Dictionary<string, object>
                {
                    ["type"] = "buttons",
                    ["buttons"] = buttonsList.Select(b =>
                    {
                        var btn = new Dictionary<string, object>
                        {
                            ["type"] = b.Type.ToLower(),
                            ["text"] = b.Text
                        };

                        if (b.Type.ToLower() == "url" && !string.IsNullOrEmpty(b.Url))
                            btn["url"] = b.Url;

                        /*if (b.Type.ToLower() == "phone_number" && !string.IsNullOrEmpty(b.PhoneNumber))
                            btn["phone_number"] = b.PhoneNumber;

                        return btn;
                    }).ToList()
                };

                componentsList.Add(buttonsComp);
            }

            // Objeto final
            var finalObj = new
            {
                name = NormalizeStrings.NormalizeTemplateName(model.Name),
                language = model.Language,
                category = model.Category,
                parameter_format = "NAMED",
                components = componentsList
            };

            return JsonSerializer.Serialize(finalObj, new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });
        }*/

        /*
        {"messaging_product":"whatsapp","to":"+528661425258","type":"template","template":{"name":"enviar_invitacion_invbovx","language":{"code":"en"},"components":[{"type":"header","parameters":[{"type":"image","image":{"link":"https://xvinvboxv.com/wp-content/uploads/2026/02/WhatsApp-Image-2026-02-19-at-4.33.06-PM-1.jpeg"}}]},{"type":"body","parameters":[{"type":"text","text":"Test"},{"type":"text","text":"Ana TEST"},{"type":"text","text":"Nuestra Boda"},{"type":"text","text":"https://xvinvboxv.com/mis-xv-regina-vazquez?codigo=0C8BE1"},{"type":"text","text":"Sábado 07 de marzo del 2026"},{"type":"text","text":"Ana TEST"}]}]}}
            */

        [HttpPost("enviar-invitacion-dina")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(Roles = "Admin,WeddingPlanner,User")]
        public async Task<IActionResult> EnviarInvitacionMasivaTemplateDinamica(
                [FromBody] MasiveSendTemplateDTO sendTemplateDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return BadRequest(new { error = "Usuario no autenticado" });

            var userWhatsAppConfig = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(userId);
            if (!userWhatsAppConfig.Success)
                return BadRequest(new { error = "Este usuario no tiene WhatsApp configurado" });

            var accessToken = userWhatsAppConfig.Result!.AccessToken;
            var phoneNumberId = userWhatsAppConfig.Result.PhoneNumberId;

            int enviados = 0;
            int fallidos = 0;
            var errores = new List<object>();

            foreach (var code in sendTemplateDTO.Codes)
            {
                try
                {
                    var invitacion = await _invitationUnitOfWork.GetByCodeAsync(code);

                    if (!invitacion.Success || invitacion.Result == null)
                    {
                        fallidos++;
                        errores.Add(new { Code = code, Error = "Invitación no encontrada" });
                        continue;
                    }

                    var ev = invitacion.Result.Event!;

                    // 🔥 Obtener plantilla desde BD
                    var template = await _templateRepository.GetByNameAsync(sendTemplateDTO.TemplateName);

                    if (template == null)
                    {
                        fallidos++;
                        errores.Add(new { Code = code, Error = "Plantilla no encontrada en BD" });
                        continue;
                    }

                    // 🔥 Construcción dinámica automática
                    var components = _templateBuilderService.BuildComponents(
                        template,
                        invitacion.Result,
                        ev,
                        code
                    );
                    if (components == null)
                        continue;
                    var fullnumber = string.Concat(invitacion.Result.CountryCode, invitacion.Result.PhoneNumber);

                    var result = await _whatsAppService.EnviarTemplateDinamicoAsync(
                        accessToken,
                        phoneNumberId,
                        fullnumber,
                        template.Name,
                        template.Language,
                        template.Content,
                        components
                    );
                    await SaveMessageHistory(invitacion.Result.Code!, result);
                    // Asociar la template enviada con la invitacion para llevar un control de que plantilla se envio a cada invitacion, y evitar enviar varias veces la misma plantilla a una misma invitacion

                    await _templateRepository.AddSentTemplateAsync(template.TemplateNumber, invitacion.Result.Id);
                    var messageDto = new WhatsappIncomingMessageDto
                    {
                        PhoneNumber = userWhatsAppConfig.Result.PhoneNumber,
                        MessageId = result.Result!.Wamid,
                        From = result.Result!.Contact,
                        Text = result.Result.Message,
                        Type = "template",
                        ReplyToMessageId = result.Result!.Wamid,
                        Direction = "OUT",
                        Status = "sent",
                        Imagen = result.Result!.Imagen,
                    };

                    var response = await _iMessageUnitOfWork
                    .AddReceiveMessages(messageDto);
                    if (!result.Success)
                    {
                        fallidos++;
                        errores.Add(new { Code = code, Error = result.Message });
                        continue;
                    }
                    enviados++;

                    await Task.Delay(1200);
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

    // The issue arises because the `ComponentRequest` class has duplicate property names for `Type` and `Format`.
    // To resolve this, we need to ensure that the properties in the `ComponentRequest` class have unique names.
    // Below is the corrected `ComponentRequest` class with unique property names.

    public class ComponentRequest
    {
        public string ComponentType { get; set; } // Renamed from 'Type' to 'ComponentType'
        public string? ComponentFormat { get; set; } // Renamed from 'Format' to 'ComponentFormat'
        public string? Text { get; set; }
        public BodyExample? Example { get; set; }
        public List<ButtonRequest>? Buttons { get; set; }
    }
}