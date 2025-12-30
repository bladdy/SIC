using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Response
{
    public class WhatsAppApiResponse
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; } = string.Empty;

        [JsonProperty("contacts")]
        public List<WhatsAppContact> Contacts { get; set; } = new();

        [JsonProperty("messages")]
        public List<WhatsAppMessage> Messages { get; set; } = new();
    }

    public class WhatsAppContact
    {
        [JsonProperty("input")]
        public string Input { get; set; } = string.Empty;

        [JsonProperty("wa_id")]
        public string WaId { get; set; } = string.Empty;
    }

    public class WhatsAppMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
    }

    public class WhatsAppErrorResponse
    {
        [JsonProperty("error")]
        public WhatsAppError Error { get; set; } = null!;
    }

    public class WhatsAppError
    {
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("fbtrace_id")]
        public string FbTraceId { get; set; } = string.Empty;
    }

    public class Error
    {
        public bool success { get; set; }
        public string message { get; set; }
        public object result { get; set; }
    }

    public class WhatsAppApiError
    {
        public Error error { get; set; }
    }
}