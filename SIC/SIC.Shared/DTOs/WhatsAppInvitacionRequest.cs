namespace SIC.Backend.DTOs
{
    public class WhatsAppInvitacionRequest
    {
        public string NumeroDestino { get; set; } = null!;
        public string TemplateName { get; set; } = null!;
        public string LanguageCode { get; set; } = "es_MX";
        public List<string>? Parametros { get; set; }
    }
}