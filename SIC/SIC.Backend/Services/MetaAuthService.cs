using SIC.Shared.DTOs;
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
            var response = await _http.PostAsync(
                "https://graph.facebook.com/v24.0/oauth/access_token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = _config["Meta:AppId"]!,
                    ["client_secret"] = _config["Meta:AppSecret"]!,
                    ["code"] = code,
                    ["redirect_uri"] = "https://www.facebook.com/connect/login_success.html"
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
                ExpiresIn = doc.GetProperty("expires_in").GetInt32()
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
    }
}