using SIC.Shared.DTOs;
using SIC.Shared.Response;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SIC.Backend.Services
{
    public class MetaAuthService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public MetaAuthService(IConfiguration config)
        {
            _http = new HttpClient();
            _config = config;
        }

        // 1️⃣ Exchange AUTH CODE → TEMP TOKEN
        public async Task<MetaToken> ExchangeCodeAsync(string code)
        {
            var appId = _config["WhatsApp:Meta_APP_ID"];
            var appSecret = _config["WhatsApp:AppSecret"];

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
                throw new Exception("❌ Configuración de Meta faltante");

            var response = await _http.PostAsync(
                "https://graph.facebook.com/v24.0/oauth/access_token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = appId,
                    ["client_secret"] = appSecret,
                    ["code"] = code
                })
            );

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Meta OAuth error: {json}");

            var doc = JsonDocument.Parse(json).RootElement;

            return new MetaToken
            {
                AccessToken = doc.GetProperty("access_token").GetString()!,
                TokenType = doc.GetProperty("token_type").GetString()!,
                ExpiresIn = doc.TryGetProperty("expires_in", out var exp)
                    ? exp.GetInt32()
                    : 0
            };
        }

        // 2️⃣ Crear SYSTEM USER
        public async Task<string> CreateSystemUserAsync(string businessId, string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v24.0/{businessId}/system_users"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            //Hue pasarle el nombre y rol del system user, aunque el rol no es tan relevante porque luego se le asignan permisos específicos
            request.Content = JsonContent.Create(new
            {
                name = "SIC WhatsApp System User",
                role = "ADMIN"
            });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Create system user failed: {json}");

            return JsonDocument.Parse(json)
                .RootElement
                .GetProperty("id")
                .GetString()!;
        }

        // 3️⃣ Generar TOKEN PERMANENTE
        public async Task<string> GeneratePermanentTokenAsync(
            string systemUserId,
            string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v24.0/{systemUserId}/access_tokens"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Content = JsonContent.Create(new
            {
                scope = new[]
                {
                "whatsapp_business_messaging",
                "whatsapp_business_management"
            }
            });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Permanent token failed: {json}");

            return JsonDocument.Parse(json)
                .RootElement
                .GetProperty("access_token")
                .GetString()!;
        }

        public async Task<WhatsAppPhoneNumberResponse> GetPhoneNumberAsync(
            string phoneNumberId,
            string accessToken)
        {
            if (string.IsNullOrEmpty(phoneNumberId))
                throw new ArgumentException("phoneNumberId es requerido");

            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("accessToken es requerido");

            var url = $"https://graph.facebook.com/v22.0/{phoneNumberId}" +
                      "?fields=id,display_phone_number,verified_name";

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error obteniendo phone number: {content}");
            }

            var json = System.Text.Json.JsonDocument.Parse(content);

            var root = json.RootElement;

            return new WhatsAppPhoneNumberResponse
            {
                Id = root.GetProperty("id").GetString(),
                DisplayPhoneNumber = root.GetProperty("display_phone_number").GetString(),
                VerifiedName = root.TryGetProperty("verified_name", out var vn)
                    ? vn.GetString()
                    : null
            };
        }

        public async Task<WhatsAppPhoneNumberResponse?> GetPhoneNumbersFromWaba(string wabaId, string accessToken)
        {
            var url = $"https://graph.facebook.com/v22.0/{wabaId}/phone_numbers";

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.GetAsync(url);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error obteniendo números: {content}");

            var json = JsonDocument.Parse(content).RootElement;

            var data = json.GetProperty("data");

            if (data.GetArrayLength() == 0)
                return null;

            var phone = data[0];

            return new WhatsAppPhoneNumberResponse
            {
                Id = phone.GetProperty("id").GetString(),
                DisplayPhoneNumber = phone.GetProperty("display_phone_number").GetString(),
                VerifiedName = phone.TryGetProperty("verified_name", out var vn)
                    ? vn.GetString()
                    : null
            };
        }

        public async Task SubscribeAppAsync(string wabaId, string accessToken)
        {
            if (string.IsNullOrEmpty(wabaId))
                throw new ArgumentException("wabaId es requerido");

            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("accessToken es requerido");

            var url = $"https://graph.facebook.com/v22.0/{wabaId}/subscribed_apps";

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.PostAsync(url, null);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error suscribiendo app al WABA: {content}");
            }

            // Opcional: log para debugging
            Console.WriteLine($"✅ WABA {wabaId} suscrito correctamente al webhook");
        }
    }
}