using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SIC.Shared.Entities;

namespace SIC.Backend.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;

        public WhatsAppService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<(bool success, string messageId, string error)> enviaAsync(string token)
        {
            // Identificador de número de teléfono
            string idTelefono = "864164460106587";
            // Nuestro teléfono
            string telefono = "528661425258";
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/v22.0/{idTelefono}/messages");
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Content = new StringContent(
                "{ \"messaging_product\": \"whatsapp\", \"to\": \"528661425258\", \"type\": \"template\", \"template\": { \"name\": \"hello_world\", \"language\": { \"code\": \"en_US\" } } }",
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

        public async Task<(bool success, string messageId, string error)> EnviarInvitacionAsync(
            string accessToken,
            string phoneNumberId,
            string numeroDestino,
            string mensaje,
            byte[] boletaPdf,
            string fileName)
        {
            try
            {
                _httpClient.BaseAddress = new Uri($"https://graph.facebook.com/v22.0/{phoneNumberId}/");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Enviar mensaje con el documento adjunto
                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = numeroDestino,
                    type = "document",
                    document = new
                    {
                        link = $"https://example.com/{fileName}", // Replace with actual file URL
                        caption = mensaje
                    }
                };

                var response = await _httpClient.PostAsync(
                    "messages",
                    new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                var json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    return (false, "", json);

                dynamic result = JsonConvert.DeserializeObject(json)!;
                string messageId = result.messages[0].id;

                return (true, messageId, "");
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
        }
    }
}