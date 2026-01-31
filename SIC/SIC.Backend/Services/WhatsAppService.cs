using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using PdfSharpCore.Pdf.Content.Objects;
using SIC.Shared.DTOs;
using SIC.Shared.Response;

namespace SIC.Backend.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;

        public WhatsAppService()
        {
            _httpClient = new HttpClient();
        }

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
                    MessageId = apiResponse!.Messages.First().Id,
                    NumeroDestino = numeroDestino,
                    TemplateName = templateName
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

        public async Task<bool> MarkMessagesAsSeenAsync(string accessToken, string phoneNumberId, MarkMessagesAsSeenDto dto)
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
                recipient = new
                {
                    id = dto.Psid
                },
                sender_action = "mark_seen"
            };

            var response = await _httpClient.PutAsJsonAsync(url, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return false;
            }

            return true;
        }
    }
}