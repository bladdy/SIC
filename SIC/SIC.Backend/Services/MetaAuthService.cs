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

        // 1️⃣ Exchange AUTH CODE → USER TOKEN (temporal)
        public async Task<MetaToken> ExchangeCodeAsync(string code)
        {
            var appId = _config["WhatsApp:Meta_APP_ID"];
            var appSecret = _config["WhatsApp:AppSecret"];

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
                throw new Exception("❌ Configuración de Meta faltante");

            var response = await _http.PostAsync(
                "https://graph.facebook.com/v25.0/oauth/access_token",
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
                ExpiresIn = doc.TryGetProperty("expires_in", out var exp) ? exp.GetInt32() : 0
            };
        }

        // 2️⃣ Suscribir la app al WABA
        // ⚠️ Requiere App Token (AppID|AppSecret), NO el User Token
        public async Task SubscribeAppAsync(string wabaId)
        {
            if (string.IsNullOrEmpty(wabaId)) throw new ArgumentException("wabaId es requerido");

            var appToken = GetAppToken(); // ← usa AppID|AppSecret directamente

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/{wabaId}/subscribed_apps"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", appToken);

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error suscribiendo app al WABA: {content}");

            Console.WriteLine($"✅ App suscrita al WABA {wabaId}");
        }

        // 3️⃣ Crear SYSTEM USER bajo el Business
        // Requiere que el userToken tenga rol admin en el Business
        public async Task<string> CreateSystemUserAsync(string businessId, string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/{businessId}/system_users"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = JsonContent.Create(new
            {
                name = "SIC WhatsApp System User",
                role = "ADMIN"
            });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error creando system user: {json}");

            return JsonDocument.Parse(json).RootElement.GetProperty("id").GetString()!;
        }

        // 4️⃣ Asignar el WABA al System User para que tenga permisos
        public async Task AssignWabaToSystemUserAsync(string wabaId, string systemUserId, string appToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/{wabaId}/assigned_users"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", appToken);
            request.Content = JsonContent.Create(new
            {
                user = systemUserId,
                tasks = new[] { "MANAGE", "DEVELOP" }
            });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error asignando WABA al system user: {json}");

            Console.WriteLine($"✅ WABA {wabaId} asignado al system user {systemUserId}");
        }

        // 5️⃣ Generar TOKEN PERMANENTE para el System User
        public async Task<string> GeneratePermanentTokenAsync(string systemUserId, string appToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/{systemUserId}/access_tokens"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", appToken);
            request.Content = JsonContent.Create(new
            {
                appsecret_proof = ComputeAppSecretProof(appToken),
                scope = "whatsapp_business_messaging,whatsapp_business_management"
            });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error generando token permanente: {json}");

            return JsonDocument.Parse(json).RootElement.GetProperty("access_token").GetString()!;
        }

        // 6️⃣ Obtener datos del phone number
        public async Task<WhatsAppPhoneNumberResponse> GetPhoneNumberAsync(string phoneNumberId, string accessToken)
        {
            if (string.IsNullOrEmpty(phoneNumberId)) throw new ArgumentException("phoneNumberId es requerido");
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentException("accessToken es requerido");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://graph.facebook.com/v25.0/{phoneNumberId}?fields=id,display_phone_number,verified_name"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error obteniendo phone number: {content}");

            var root = JsonDocument.Parse(content).RootElement;

            return new WhatsAppPhoneNumberResponse
            {
                Id = root.GetProperty("id").GetString(),
                DisplayPhoneNumber = root.GetProperty("display_phone_number").GetString(),
                VerifiedName = root.TryGetProperty("verified_name", out var vn) ? vn.GetString() : null
            };
        }

        // 7️⃣ Enviar mensaje de prueba con plantilla hello_world
        public async Task<string> SendTestMessageAsync(string phoneNumberId, string toPhone, string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/{phoneNumberId}/messages"
            );
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Content = JsonContent.Create(new
            {
                messaging_product = "whatsapp",
                to = toPhone,
                type = "template",
                template = new
                {
                    name = "hello_world",
                    language = new { code = "en_US" }
                }
            });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error enviando mensaje de prueba: {json}");

            Console.WriteLine($"✅ Mensaje enviado a {toPhone}");
            return json;
        }

        // Helper: App Token = AppID|AppSecret (para operaciones que requieren permisos de app)
        public string GetAppToken()
        {
            var appId = _config["WhatsApp:Meta_APP_ID"];
            var token = _config["WhatsApp:AccessToken"]; // ← opcional, si se configuró un token directo lo usa (útil para pruebas)
            var appSecret = _config["WhatsApp:AppSecret"];
            return $"{appId}|{appSecret}";
        }

        // Helper: HMAC-SHA256 para appsecret_proof (requerido por algunas APIs de Meta)
        private string ComputeAppSecretProof(string accessToken)
        {
            var appSecret = _config["WhatsApp:AppSecret"]!;
            var key = System.Text.Encoding.UTF8.GetBytes(appSecret);
            var msg = System.Text.Encoding.UTF8.GetBytes(accessToken);
            using var hmac = new System.Security.Cryptography.HMACSHA256(key);
            var hash = hmac.ComputeHash(msg);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}