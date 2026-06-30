using Newtonsoft.Json;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Helpers;
using SIC.Shared.Request;
using SIC.Shared.Response;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static QRCoder.PayloadGenerator;

namespace SIC.Backend.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;

        public WhatsAppService()
        {
            _httpClient = new HttpClient();
        }

        //Dinamico

        public async Task<ActionResponse<WhatsAppMessageResponse>> EnviarTemplateDinamicoAsync(
            string accessToken,
            string phoneNumberId,
            string numeroDestino,
            string templateName,
            string languageCode,
            string templateContent,
            List<TemplateComponentRequest> components)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var templateComponents = new List<object>();

                foreach (var component in components)
                {
                    // 🔥 BUTTON
                    if (component.Type == "button")
                    {
                        var buttonParams = new List<object>();

                        if (component.Parameters != null)
                        {
                            foreach (var p in component.Parameters)
                            {
                                buttonParams.Add(new
                                {
                                    type = "text",
                                    text = p.Text
                                });
                            }
                        }

                        templateComponents.Add(new
                        {
                            type = "button",
                            sub_type = component.SubType,
                            index = component.Index?.ToString(),
                            parameters = buttonParams
                        });

                        continue;
                    }

                    // 🔥 HEADER o BODY
                    var parametersList = new List<object>();

                    if (component.Parameters != null)
                    {
                        foreach (var p in component.Parameters)
                        {
                            if (p.Type == "text")
                            {
                                parametersList.Add(new
                                {
                                    type = "text",
                                    text = p.Text
                                });
                            }
                            else if (p.Type == "image")
                            {
                                parametersList.Add(new
                                {
                                    type = "image",
                                    image = new { link = p.Link }
                                });
                            }
                            else if (p.Type == "video")
                            {
                                parametersList.Add(new
                                {
                                    type = "video",
                                    video = new { link = p.Link }
                                });
                            }
                            else if (p.Type == "document")
                            {
                                parametersList.Add(new
                                {
                                    type = "document",
                                    document = new { link = p.Link }
                                });
                            }
                        }
                    }

                    templateComponents.Add(new
                    {
                        type = component.Type,
                        parameters = parametersList
                    });
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = numeroDestino,
                    type = "template",
                    template = new
                    {
                        name = templateName,
                        language = new { code = languageCode },
                        components = templateComponents
                    }
                };

                var jsonPayload = JsonConvert.SerializeObject(
                    payload,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                );

                var response = await _httpClient.PostAsync(
                    $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages",
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                );

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ActionResponse<WhatsAppMessageResponse>
                    {
                        Success = false,
                        Message = json
                    };
                }
                string finalMessage = WhatsAppTemplateParser.ReplaceTemplateVariables(
                    jsonPayload,
                    templateContent
                );
                var apiResponse = JsonConvert.DeserializeObject<WhatsAppApiResponse>(json);
                var imagenFromComponents = components
                            .SelectMany(c => c.Parameters ?? new List<TemplateParameterRequest>())
                            .FirstOrDefault(p => p.Type == "image")?.Link ?? "";
                return new ActionResponse<WhatsAppMessageResponse>
                {
                    Success = true,
                    Result = new WhatsAppMessageResponse
                    {
                        Wamid = apiResponse!.Messages.First().Id,
                        NumeroDestino = numeroDestino,
                        TemplateName = templateName,
                        Contact = apiResponse.Contacts.First().WaId,
                        Message = finalMessage,
                        Imagen = imagenFromComponents
                    }
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<WhatsAppMessageResponse>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        //Estatico
        public async Task<ActionResponse<WhatsAppMessageResponse>> EnviarInvitacionAsync(
            string accessToken,
            string phoneNumberId,
            string numeroDestino,
            string templateName,
            string languageCode,
            string coverImageUrl,
            List<string>? parametros = null)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var templatePayload = new
                {
                    name = templateName,
                    language = new { code = languageCode },
                    components = new List<object>
                    {
                        // HEADER IMAGE
                        new
                        {
                            type = "header",
                            parameters = new[]
                            {
                                new
                                {
                                    type = "image",
                                    image = new
                                    {
                                        link = coverImageUrl
                                    }
                                }
                            }
                        },

                        // BODY
                        parametros != null && parametros.Any()
                            ? new
                            {
                                type = "body",
                                parameters = parametros.Select(p => new
                                {
                                    type = "text",
                                    text = p
                                }).ToArray()
                            }
                            : null
                    }
                    .Where(c => c != null) // evita nulls
                    .ToArray()
                };

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = numeroDestino,
                    type = "template",
                    template = templatePayload
                };

                var jsonPayload = JsonConvert.SerializeObject(
                    payload,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
                );

                var response = await _httpClient.PostAsync(
                    $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages",
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                );

                var json = await response.Content.ReadAsStringAsync();

                // ❌ Error devuelto por Meta / WhatsApp
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = JsonConvert.DeserializeObject<WhatsAppErrorResponse>(json);

                    var errorMessage = errorResponse?.Error != null
                        ? $"[{errorResponse.Error.Code}] {errorResponse.Error.Message}"
                        : json;

                    return new ActionResponse<WhatsAppMessageResponse>
                    {
                        Success = false,
                        Message = errorMessage
                    };
                }

                // ✅ Respuesta exitosa
                var apiResponse = JsonConvert.DeserializeObject<WhatsAppApiResponse>(json);

                var responseObject = new WhatsAppMessageResponse
                {
                    Wamid = apiResponse!.Messages.First().Id,
                    NumeroDestino = numeroDestino,
                    TemplateName = templateName,
                    Contact = apiResponse.Contacts.First().WaId
                };

                return new ActionResponse<WhatsAppMessageResponse>
                {
                    Result = responseObject,
                    Success = true,
                    Message = "Mensaje enviado correctamente"
                };
            }
            catch (Exception ex)
            {
                return new ActionResponse<WhatsAppMessageResponse>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<(bool success, string messageId, string error)> enviaAsync(string token)
        {
            token = "EAAUu8FHu8ZAwBP4g5ehkX4FzNUIQNEtZAPqY3vhxS3yM1cNiNHZCnOwwBzhKKTUkiURBprbA80QubiVZBVmUZAFf0XNMLQzAXd4AaLLBTDrj6C4UeqpZBIMA9l4KdGWot7aNwXZCI75b5O1ZCZCWVu48EoVkQtW7EPJNl66fu7k2meHnbvnSKb6XwADXA95eMrBBTEoaD1JxnHiewVwGZA0CRzas8EzifRUZB7KBddJd5quNKDQnDZA489yS2mTYq6YZD";
            // Identificador de número de teléfono
            string idTelefono = "909558215567844";
            // Nuestro teléfono
            string telefono = "528661425258";
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/v22.0/{idTelefono}/messages");
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Content = new StringContent(
                "{ \"messaging_product\": \"whatsapp\", " +
                "\"to\": \"528661425258\", " +
                "\"type\": \"template\", " +
                "\"template\": { " +
                    "\"name\": \"invbox_test\", " +
                    "\"language\": { \"code\": \"en\" } " +
                "} " +
                "}",
                Encoding.UTF8,
                "application/json"
            );

            HttpResponseMessage response = await client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            return (true, "mensaje", "");
        }

        public async Task<(bool success, string messageId, string error)> EnviarTextoAsync(
            string accessToken,
            string phoneNumberId,
            string numeroDestino,
            string mensaje)
        {
            numeroDestino = "528661425258";
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var payload1 = new
                {
                    messaging_product = "whatsapp",
                    to = numeroDestino,
                    type = "text",
                    text = new
                    {
                        body = mensaje
                    }
                };

                var payload = new
                {
                    messaging_product = "whatsapp",
                    recipient_type = "individual",
                    to = numeroDestino,
                    type = "text",
                    text = new
                    {
                        preview_url = true,  // true o false según quieras mostrar vista previa de enlaces
                        body = mensaje       // tu texto
                    }
                };

                var jsonPayload = JsonConvert.SerializeObject(payload);

                var response = await _httpClient.PostAsync(
                    $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages",
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                );

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return (false, "", $"Error al enviar mensaje: {json}");
                }

                dynamic result = JsonConvert.DeserializeObject(json)!;
                string messageId = result.messages[0].id;

                return (true, messageId, "");
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
        }

        public async Task<WhatsappTemplates?> GetTemplatesAsync(
            UsuarioWhatsAppConfig? usuarioWhatsApp)
        {
            if (string.IsNullOrWhiteSpace(usuarioWhatsApp!.WabaId))
                throw new ArgumentException("WABA_ID no configurado");

            if (string.IsNullOrWhiteSpace(usuarioWhatsApp!.AccessToken))
                throw new ArgumentException("Access Token no configurado");

            using var http = new HttpClient();

            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", usuarioWhatsApp.AccessToken);

            var url =
                $"https://graph.facebook.com/v22.0/{usuarioWhatsApp.WabaId}/message_templates";

            var response = await http.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error WhatsApp API: {json}");

            var result = System.Text.Json.JsonSerializer.Deserialize<WhatsappTemplates>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result ?? throw new InvalidOperationException("Failed to deserialize WhatsappTemplates.");
        }

        public async Task<string?> SendTextMessageAsync(string accessToken, string phoneNumberId, SendWhatsappMessageDto dto)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token is required", nameof(accessToken));
            if (string.IsNullOrEmpty(phoneNumberId))
                throw new ArgumentException("Phone number ID is required", nameof(phoneNumberId));
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            using var httpClient = new HttpClient();

            // Endpoint de la API de WhatsApp Cloud
            var url = $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages";

            // Payload de la API
            var payload = new
            {
                messaging_product = "whatsapp",
                to = dto.PhoneNumber,
                type = "text",
                text = new
                {
                    body = dto.Message
                }
            };

            // Configurar encabezados
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Enviar POST
            var response = await httpClient.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Error sending WhatsApp message: {response.StatusCode} - {errorContent}");
            }

            // Obtener la respuesta JSON
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Opcional: devolver el message ID generado por WhatsApp
            using var document = JsonDocument.Parse(jsonResponse);
            if (document.RootElement.TryGetProperty("messages", out var messagesArray) && messagesArray.GetArrayLength() > 0)
            {
                return messagesArray[0].GetProperty("id").GetString();
            }

            return null;
        }

        public async Task<bool> MarkMessageAsReadAsync(
            string accessToken,
            string phoneNumberId,
            string messageId)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token is required");
            if (string.IsNullOrEmpty(phoneNumberId))
                throw new ArgumentException("Phone number ID is required");
            if (string.IsNullOrEmpty(messageId))
                throw new ArgumentException("Message ID is required");

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"https://graph.facebook.com/v22.0/{phoneNumberId}/messages";

            var payload = new
            {
                messaging_product = "whatsapp",
                status = "read",
                message_id = messageId
            };

            var response = await httpClient.PostAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine(error);
                return false;
            }

            return true;
        }

        public async Task<bool> CreateWhatsAppTemplateAsync(
            string accessToken,
            string wabaId,
            string requestJson,
            CreateTemplateModel model)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required");

            if (string.IsNullOrWhiteSpace(wabaId))
                throw new ArgumentException("wabaId is required");

            // ============================================
            // PARSE JSON
            // ============================================

            var requestObj = JsonNode.Parse(requestJson);

            if (requestObj == null)
                throw new Exception("No se pudo parsear requestJson");

            var components = requestObj["components"]?.AsArray();

            if (components == null)
                throw new Exception("El JSON no contiene components");

            // ============================================
            // HEADER
            // ============================================

            var header = components
                .FirstOrDefault(c =>
                    c?["type"]?.ToString()
                        .Equals("HEADER", StringComparison.OrdinalIgnoreCase) == true);

            if (header != null)
            {
                // ========================================
                // HEADER TEXTO
                // ========================================

                if (model.Header?.Type == "TEXT")
                {
                    header["format"] = "TEXT";
                    header["text"] = model.Header.Text;
                }

                // ========================================
                // HEADER MEDIA
                // ========================================
                else if (
                    model.MediaType == "IMAGE"
                    || model.MediaType == "VIDEO"
                    || model.MediaType == "DOCUMENT")
                {
                    if (string.IsNullOrWhiteSpace(model.MediaUrl))
                        throw new Exception("MediaUrl es requerido.");

                    // ====================================
                    // SUBIR MEDIA A META
                    // ====================================

                    var uploadedHandle =
                        await UploadTemplateImageAsync(
                            accessToken,
                            model.MediaUrl);

                    if (string.IsNullOrWhiteSpace(uploadedHandle))
                        throw new Exception("No se pudo obtener el media handle.");

                    header["format"] = model.MediaType;

                    JsonObject exampleObject;

                    if (header["example"] == null)
                    {
                        exampleObject = new JsonObject();

                        header["example"] = exampleObject;
                    }
                    else
                    {
                        exampleObject = header["example"]!.AsObject();
                    }

                    exampleObject["header_handle"] = new JsonArray
                    {
                        uploadedHandle
                    };
                }
            }

            // ============================================
            // BODY
            // ============================================

            var body = components
                .FirstOrDefault(c =>
                    c?["type"]?.ToString()
                        .Equals("BODY", StringComparison.OrdinalIgnoreCase) == true);

            if (body != null)
            {
                var bodyText = body["text"]?.ToString() ?? "";

                body["text"] = bodyText;

                // ========================================
                // BODY EXAMPLES POSITIONAL
                // ========================================

                var bodyExamples = new JsonArray();

                var bodyParams =
                    model.Components
                        .FirstOrDefault(x => x.Type == "BODY")
                        ?.BodyExampleParams;

                if (bodyParams != null)
                {
                    foreach (var param in bodyParams)
                    {
                        if (string.IsNullOrWhiteSpace(param.ExampleValue))
                            continue;

                        bodyExamples.Add(param.ExampleValue);
                    }
                }

                if (bodyExamples.Count > 0)
                {
                    body["example"] = new JsonObject
                    {
                        ["body_text"] = new JsonArray
                        {
                            bodyExamples
                        }
                    };
                }
            }

            // ============================================
            // FOOTER
            // ============================================

            var footer = components
                .FirstOrDefault(c =>
                    c?["type"]?.ToString()
                        .Equals("FOOTER", StringComparison.OrdinalIgnoreCase) == true);

            if (footer != null)
            {
                footer["text"] = model.Footer ?? "";
            }

            // ============================================
            // BUTTONS
            // ============================================

            var buttons = components
                .FirstOrDefault(c =>
                    c?["type"]?.ToString()
                        .Equals("BUTTONS", StringComparison.OrdinalIgnoreCase) == true);

            if (buttons?["buttons"] is JsonArray buttonsArray)
            {
                var validButtons = new JsonArray();

                for (int i = 0; i < buttonsArray.Count; i++)
                {
                    var originalBtn = buttonsArray[i];

                    var modelButton =
                        model.Buttons.ElementAtOrDefault(i);

                    if (originalBtn == null || modelButton == null)
                        continue;

                    // ====================================
                    // VALIDAR TEXTO
                    // ====================================

                    if (string.IsNullOrWhiteSpace(modelButton.Text))
                        continue;

                    // ====================================
                    // CLONAR BOTON
                    // ====================================

                    var btn = JsonNode.Parse(
                        originalBtn.ToJsonString())!.AsObject();

                    // ====================================
                    // QUICK_REPLY
                    // ====================================

                    if (modelButton.Type == "QUICK_REPLY")
                    {
                        btn["type"] = "QUICK_REPLY";
                        btn["text"] = modelButton.Text;

                        btn.Remove("url");
                        btn.Remove("example");

                        validButtons.Add(btn);

                        continue;
                    }

                    // ====================================
                    // URL
                    // ====================================

                    if (modelButton.Type == "URL")
                    {
                        btn["type"] = "URL";
                        btn["text"] = modelButton.Text;

                        // ================================
                        // STATIC
                        // ================================

                        if (modelButton.UrlType == "STATIC")
                        {
                            if (string.IsNullOrWhiteSpace(modelButton.Url))
                                continue;

                            btn["url"] = modelButton.Url;

                            btn.Remove("example");

                            validButtons.Add(btn);

                            continue;
                        }

                        // ================================
                        // DYNAMIC
                        // ================================

                        if (modelButton.UrlType == "DYNAMIC")
                        {
                            if (string.IsNullOrWhiteSpace(modelButton.UrlBase))
                                continue;

                            // ====================================
                            // POSITIONAL PARAM
                            // ====================================

                            btn["url"] =
                                $"{modelButton.UrlBase}{{{{1}}}}";

                            btn["example"] = new JsonArray
                    {
                        modelButton.DynamicExample ?? "123456"
                    };

                            validButtons.Add(btn);

                            continue;
                        }
                    }
                }

                // ========================================
                // REEMPLAZAR BOTONES
                // ========================================

                buttons["buttons"] = validButtons;
            }

            // ============================================
            // PARAMETER FORMAT
            // ============================================

            requestObj["parameter_format"] = "POSITIONAL";

            // ============================================
            // JSON FINAL
            // ============================================

            var finalJson = requestObj.ToJsonString(
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            Console.WriteLine("============== JSON FINAL ==============");
            Console.WriteLine(finalJson);

            // ============================================
            // HTTP
            // ============================================

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            var url =
                $"https://graph.facebook.com/v22.0/{wabaId}/message_templates";

            var content = new StringContent(
                finalJson,
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(
                url,
                content);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            Console.WriteLine("============== RESPONSE META ==============");
            Console.WriteLine(responseBody);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("============== ERROR META ==============");
                Console.WriteLine(responseBody);

                return false;
            }

            Console.WriteLine("============== TEMPLATE CREADO ==============");
            Console.WriteLine(responseBody);

            return true;
        }

        public async Task CreateTemplateAsync(
                UsuarioWhatsAppConfig config,
                object template)
        {
            var json = template switch
            {
                string s => s,
                _ => System.Text.Json.JsonSerializer.Serialize(template)
            };

            var root = JsonNode.Parse(json)!.AsObject();

            root = await ReplaceHeaderHandleAsync(
                root,
                config.AccessToken,
                "https://invboxv.com/wp-content/uploads/2026/05/WhatsApp-Image-2026-05-28-at-7.22.03-PM.jpeg");

            // 🔥 convertir de vuelta a string JSON
            var finalJson = root.ToJsonString();

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.AccessToken);

            var response = await client.PostAsync(
                $"https://graph.facebook.com/v23.0/{config.WabaId}/message_templates",
                new StringContent(finalJson, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
        }

        private async Task<string> UploadTemplateImageAsync(
            string accessToken,
            string imageUrl)
        {
            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            // ============================================
            // DESCARGAR IMAGEN
            // ============================================

            var imageBytes =
                await httpClient.GetByteArrayAsync(imageUrl);

            // ============================================
            // PASO 1:
            // CREAR UPLOAD SESSION
            // ============================================

            var fileLength = imageBytes.Length;

            var createSessionUrl =
                $"https://graph.facebook.com/v22.0/app/uploads" +
                $"?file_name=header.jpg" +
                $"&file_length={fileLength}" +
                $"&file_type=image/jpeg";

            var createResponse =
                await httpClient.PostAsync(
                    createSessionUrl,
                    null);

            var createBody =
                await createResponse.Content.ReadAsStringAsync();

            Console.WriteLine("======= CREATE SESSION =======");
            Console.WriteLine(createBody);

            if (!createResponse.IsSuccessStatusCode)
                throw new Exception(createBody);

            var createJson = JsonNode.Parse(createBody);

            var uploadId = createJson?["id"]?.ToString();

            if (string.IsNullOrWhiteSpace(uploadId))
                throw new Exception("Meta no devolvió upload session id");

            // ============================================
            // PASO 2:
            // SUBIR BINARIO
            // ============================================

            using var uploadContent =
                new ByteArrayContent(imageBytes);

            uploadContent.Headers.ContentType =
                new MediaTypeHeaderValue("image/jpeg");

            var uploadRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"https://graph.facebook.com/v22.0/{uploadId}");

            uploadRequest.Content = uploadContent;

            uploadRequest.Headers.Add(
                "file_offset",
                "0");

            var uploadResponse =
                await httpClient.SendAsync(uploadRequest);

            var uploadBody =
                await uploadResponse.Content.ReadAsStringAsync();

            Console.WriteLine("======= UPLOAD RESPONSE =======");
            Console.WriteLine(uploadBody);

            if (!uploadResponse.IsSuccessStatusCode)
                throw new Exception(uploadBody);

            // ============================================
            // META DEVUELVE EL HANDLE FINAL EN "h"
            // ============================================

            var uploadJson = JsonNode.Parse(uploadBody);

            var handle =
                uploadJson?["h"]?.ToString();

            if (string.IsNullOrWhiteSpace(handle))
                throw new Exception(
                    "Meta no devolvió header handle.");

            return handle;
        }

        private async Task<JsonObject> ReplaceHeaderHandleAsync(
            JsonObject root,
            string accessToken,
            string imageUrl)
        {
            var components = root["components"]?.AsArray();

            if (components == null)
                return root;

            foreach (var component in components)
            {
                if (component is not JsonObject obj)
                    continue;

                if (obj["type"]?.GetValue<string>() == "HEADER"
                    && obj["format"]?.GetValue<string>() == "IMAGE")
                {
                    var handle = await UploadTemplateImageAsync(
                        accessToken,
                        imageUrl);

                    obj["example"] = new JsonObject
                    {
                        ["header_handle"] = new JsonArray(handle)
                    };

                    break;
                }
            }

            return root;
        }

        public async Task<string> UploadTemplateImageAsyncsWSD(string accessToken, string imageUrl)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            // 1️⃣ Descargar imagen
            var imageResponse = await httpClient.GetAsync(imageUrl);
            if (!imageResponse.IsSuccessStatusCode)
                throw new Exception("No se pudo descargar la imagen.");

            var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
            var fileLength = imageBytes.Length;

            var contentType = imageResponse.Content.Headers.ContentType?.MediaType
                              ?? "image/jpeg";

            // Detectar extensión real
            var extension = contentType.Contains("png") ? "png" : "jpg";
            var fileName = $"header.{extension}";

            // 2️⃣ Crear sesión upload
            var createSessionBody = new
            {
                file_name = fileName,
                file_length = fileLength,
                file_type = contentType
            };

            var sessionResponse = await httpClient.PostAsync(
                "https://graph.facebook.com/v22.0/app/uploads",
                new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(createSessionBody),
                    Encoding.UTF8,
                    "application/json"));

            var sessionJson = await sessionResponse.Content.ReadAsStringAsync();

            if (!sessionResponse.IsSuccessStatusCode)
                throw new Exception("Error creando sesión upload: " + sessionJson);

            var sessionData = JsonDocument.Parse(sessionJson);
            var uploadSessionId = sessionData.RootElement
                .GetProperty("id")
                .GetString();

            if (string.IsNullOrWhiteSpace(uploadSessionId))
                throw new Exception("Meta no devolvió upload session id.");

            // 3️⃣ Subir archivo correctamente como multipart/form-data
            var uploadUrl = $"https://graph.facebook.com/v22.0/{uploadSessionId}";

            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            // IMPORTANTE: el nombre del campo debe ser "file"
            form.Add(fileContent, "file", fileName);

            var uploadResponse = await httpClient.PostAsync(uploadUrl, form);
            var uploadJson = await uploadResponse.Content.ReadAsStringAsync();

            if (!uploadResponse.IsSuccessStatusCode)
                throw new Exception("Error subiendo imagen: " + uploadJson);

            var uploadData = JsonDocument.Parse(uploadJson);

            var mediaHandle = uploadData.RootElement
                .GetProperty("h")
                .GetString();

            if (string.IsNullOrWhiteSpace(mediaHandle))
                throw new Exception("Meta no devolvió media handle.");

            return mediaHandle;
        }
    }
}

/*

{
  "name": "confirmacion_internas",
  "nanguage": "es_ES",
  "category": "MARKETING",
  "components": [
    {
      "type": "HEADER",
      "format": "TEXT",
      "text": "Invitación"
    },
    {
      "type": "BODY",
      "text": "Hola {{1}}, te informamos que tu pedido con número {{2}} ya se encuentra listo para ser retirado en nuestra sucursal.",
      "example": {
        "body_text": [
          ["Juan", "ABC123"]
        ]
      }
    },
    {
      "type": "FOOTER",
      "text": "Te esperamos"
    },
    {
      "type": "BUTTONS",
      "buttons": [
        {
          "type": "URL",
          "text": "Confirmar",
          "url": "https://www.luckyshrub.com/shop?promo={{1}}",
          "example": [
            "summer2023"
          ]
        }
      ]
    }
  ]
}

{
  "name": "promo_descuento_febrero",
  "language": "es_ES",
  "category": "MARKETING",
  "header": {
    "type": "image",
    "text": ""
  },
  "components": [
    {
      "type": "body",
      "format": "text",
      "text": "Hola {{nombre}}, tenemos un {{porcentaje}} de descuento exclusivo para ti. Usa el código {{codigo}} antes del {{fecha_limite}}.",
      "bodyExampleParams": [
        {
          "paramName": "nombre",
          "exampleValue": "Carlos"
        },
        {
          "paramName": "porcentaje",
          "exampleValue": "25%"
        },
        {
          "paramName": "codigo",
          "exampleValue": "FEB25"
        },
        {
          "paramName": "fecha_limite",
          "exampleValue": "28/02/2026"
        }
      ],
      "buttons": [
        {
          "type": "url",
          "text": "Comprar ahora",
          "url": "https://midominio.com/oferta/{{codigo}}",
          "urlType": "DYNAMIC",
          "urlBase": "https://midominio.com/oferta/",
          "dynamicExample": "FEB25",
          "example": [
            "FEB25"
          ]
        },
        {
          "type": "quick_reply",
          "text": "No me interesa",
          "url": "",
          "urlType": "",
          "urlBase": "",
          "dynamicExample": "",
          "example": []
        }
      ]
    }
  ],
  "footer": "Promoción válida por tiempo limitado.",
  "bodyExamples": [],
  "bodyExampleTypes": [],
  "buttons": [
    {
      "type": "url",
      "text": "Comprar ahora",
      "urlType": "DYNAMIC",
      "url": "https://midominio.com/oferta/{{codigo}}",
      "phoneNumber": "",
      "urlBase": "https://midominio.com/oferta/",
      "dynamicExample": "FEB25"
    },
    {
      "type": "quick_reply",
      "text": "No me interesa",
      "urlType": "",
      "url": "",
      "phoneNumber": "",
      "urlBase": "",
      "dynamicExample": ""
    }
  ],
  "mediaType": "IMAGE",
  "mediaUrl": "https://invboxv-app.com/files/5AYWC5/5a6049d9-ae7e-4d04-aaeb-527263f340a7.jpg",
  "mediaCaption": ""
}

*/