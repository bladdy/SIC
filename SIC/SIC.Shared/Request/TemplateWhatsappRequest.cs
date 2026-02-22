using System.Text.Json.Serialization;

namespace SIC.Shared.Request
{
    public class TemplateWhatsappRequest
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string Category { get; set; }
        public List<ComponentRequest> Components { get; set; }
    }

    public class ComponentRequesta
    {
        public string Type { get; set; }
        public string Format { get; set; }
        public string Text { get; set; }
        public ExampleRequest Example { get; set; }
        public List<ButtonRequest> Buttons { get; set; }
    }

    public class ExampleRequest
    {
        public List<List<string>> Body_text { get; set; }
    }

    public class ButtonRequesta
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public List<string> Example { get; set; }
    }
}