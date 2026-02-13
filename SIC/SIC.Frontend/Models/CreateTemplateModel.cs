namespace SIC.Frontend.Models
{
    public class CreateTemplateModel
    {
        public string Name { get; set; } = "";
        public string Language { get; set; } = "es_ES";

        public HeaderModel? Header { get; set; }

        public string Body { get; set; } = "";
        public string? Footer { get; set; }

        public List<string> BodyExamples { get; set; } = new();

        public List<ButtonModel> Buttons { get; set; } = new();

        // ✅ AGREGAR ESTO
        public string MediaType { get; set; } = ""; // IMAGE / VIDEO / DOCUMENT

        public string? MediaUrl { get; set; }
        public string? MediaCaption { get; set; }
    }

    public class HeaderModel
    {
        public string Type { get; set; } = "TEXT"; // TEXT / IMAGE / VIDEO / DOCUMENT
        public string? Text { get; set; }
    }

    public class ButtonModel
    {
        public string Type { get; set; } = "URL"; // URL / QUICK_REPLY

        public string Text { get; set; } = "";

        // URL only
        public string UrlType { get; set; } = "STATIC"; // STATIC / DYNAMIC

        public string? Url { get; set; }
        public string? UrlBase { get; set; }
        public string? DynamicExample { get; set; }
    }
}