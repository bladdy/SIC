using SIC.Shared.DTOs;
using SIC.Shared.Response;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
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

        // ============================================================
        // 1️⃣ Exchange AUTH CODE → SHORT-LIVED TOKEN
        // ============================================================

        public async Task<MetaToken> ExchangeCodeAsync(string code)
        {
            var appId = _config["WhatsApp:Meta_APP_ID"];
            var appSecret = _config["WhatsApp:AppSecret"];

            if (string.IsNullOrEmpty(appId) ||
                string.IsNullOrEmpty(appSecret))
            {
                throw new Exception("Configuración Meta faltante");
            }

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
                ExpiresIn = doc.TryGetProperty("expires_in", out var exp)
                    ? exp.GetInt32()
                    : 0
            };
        }

        // ============================================================
        // 2️⃣ SHORT-LIVED → LONG-LIVED TOKEN (~60 días)
        // ============================================================

        public async Task<MetaToken> ExchangeForLongLivedTokenAsync(
            string shortLivedToken)
        {
            var appId = _config["WhatsApp:Meta_APP_ID"];
            var appSecret = _config["WhatsApp:AppSecret"];

            var url =
                $"https://graph.facebook.com/v25.0/oauth/access_token" +
                $"?grant_type=fb_exchange_token" +
                $"&client_id={appId}" +
                $"&client_secret={appSecret}" +
                $"&fb_exchange_token={shortLivedToken}";

            var response = await _http.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(
                    $"Error generando long-lived token: {json}");

            var root = JsonDocument.Parse(json).RootElement;

            return new MetaToken
            {
                AccessToken = root.GetProperty("access_token").GetString()!,
                TokenType = root.TryGetProperty("token_type", out var tt)
                    ? tt.GetString()!
                    : "bearer",
                ExpiresIn = root.TryGetProperty("expires_in", out var exp)
                    ? exp.GetInt32()
                    : 0
            };
        }

        // ============================================================
        // 3️⃣ Suscribir app al WABA
        // ============================================================

        public async Task SubscribeAppAsync(
            string wabaId,
            string accessToken)
        {
            if (string.IsNullOrEmpty(wabaId))
                throw new ArgumentException("wabaId es requerido");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/{wabaId}/subscribed_apps"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _http.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error suscribiendo app al WABA: {content}");
            }

            Console.WriteLine($"✅ App suscrita al WABA {wabaId}");
        }

        // ============================================================
        // 4️⃣ Obtener datos del Phone Number
        // ============================================================

        public async Task<WhatsAppPhoneNumberResponse>
            GetPhoneNumberAsync(
                string phoneNumberId,
                string accessToken)
        {
            if (string.IsNullOrEmpty(phoneNumberId))
                throw new ArgumentException(
                    "phoneNumberId es requerido");

            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException(
                    "accessToken es requerido");

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://graph.facebook.com/v25.0/{phoneNumberId}" +
                $"?fields=id,display_phone_number,verified_name"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            var response = await _http.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error obteniendo phone number: {content}");
            }

            var root = JsonDocument.Parse(content).RootElement;

            return new WhatsAppPhoneNumberResponse
            {
                Id = root.GetProperty("id").GetString(),

                DisplayPhoneNumber =
                    root.GetProperty("display_phone_number")
                        .GetString(),

                VerifiedName =
                    root.TryGetProperty("verified_name", out var vn)
                        ? vn.GetString()
                        : null
            };
        }

        // ============================================================
        // 5️⃣ Obtener información del token
        // ============================================================

        public async Task<JsonElement> DebugTokenAsync(
            string accessToken)
        {
            var appToken = GetAppToken();

            var url =
                $"https://graph.facebook.com/debug_token" +
                $"?input_token={accessToken}" +
                $"&access_token={appToken}";

            var response = await _http.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error debug token: {json}");

            return JsonDocument.Parse(json)
                .RootElement
                .GetProperty("data");
        }

        // ============================================================
        // 6️⃣ Verificar si token expiró
        // ============================================================

        public async Task<bool> IsTokenValidAsync(
            string accessToken)
        {
            try
            {
                var data = await DebugTokenAsync(accessToken);

                if (!data.TryGetProperty("is_valid", out var valid))
                    return false;

                return valid.GetBoolean();
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // 7️⃣ Enviar mensaje de prueba
        // ============================================================

        public async Task<string> SendTestMessageAsync(
            string phoneNumberId,
            string toPhone,
            string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/" +
                $"{phoneNumberId}/messages"
            );

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            request.Content = JsonContent.Create(new
            {
                messaging_product = "whatsapp",

                to = toPhone,

                type = "template",

                template = new
                {
                    name = "hello_world",

                    language = new
                    {
                        code = "en_US"
                    }
                }
            });

            var response = await _http.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"Error enviando mensaje: {json}");
            }

            Console.WriteLine($"✅ Mensaje enviado a {toPhone}");

            return json;
        }

        // ============================================================
        // 8️⃣ Obtener App Token
        // ============================================================

        public string GetAppToken()
        {
            var appId = _config["WhatsApp:Meta_APP_ID"];
            var appSecret = _config["WhatsApp:AppSecret"];

            return $"{appId}|{appSecret}";
        }

        // ============================================================
        // 9️⃣ App Secret Proof
        // ============================================================

        private string ComputeAppSecretProof(
            string accessToken)
        {
            var appSecret =
                _config["WhatsApp:AppSecret"]!;

            var key =
                Encoding.UTF8.GetBytes(appSecret);

            var msg =
                Encoding.UTF8.GetBytes(accessToken);

            using var hmac = new HMACSHA256(key);

            var hash = hmac.ComputeHash(msg);

            return Convert.ToHexString(hash).ToLower();
        }

        // ============================================================
        // 100 Register number
        // ============================================================
        public async Task RegisterPhoneNumberAsync(
        string phoneNumberId,
        string accessToken,
        string pin)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.facebook.com/v25.0/{phoneNumberId}/register");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            request.Content = JsonContent.Create(new
            {
                messaging_product = "whatsapp",
                pin = pin
            });

            var response = await _http.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            Console.WriteLine("✅ Número registrado");
        }

        // ============================================================
        // 110 Check if phone number is registered
        // ============================================================
        public async Task<bool> IsPhoneNumberRegisteredAsync(
            string phoneNumberId,
            string accessToken)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://graph.facebook.com/v25.0/{phoneNumberId}" +
                "?fields=id,display_phone_number,verified_name,name_status,code_verification_status,quality_rating");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            var response = await _http.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(json);

            Console.WriteLine($"📥 Phone Info: {json}");

            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            // OJO:
            // Esto es solamente un ejemplo. Debes revisar qué campos
            // devuelve tu número para decidir la lógica.

            if (root.TryGetProperty("code_verification_status", out var status))
            {
                Console.WriteLine($"Verification Status: {status.GetString()}");
            }

            if (root.TryGetProperty("name_status", out var nameStatus))
            {
                Console.WriteLine($"Name Status: {nameStatus.GetString()}");
            }

            // Temporalmente devolvemos false.
            // La lógica real dependerá de la respuesta de Meta.
            return false;
        }
    }
}