using System.Text.Json.Serialization;

namespace SIC.Shared.DTOs
{
    public class WhatsappTemplates
    {
        [JsonPropertyName("data")]
        public List<TemplateDatum>? Data { get; set; }

        [JsonPropertyName("paging")]
        public Paging? Paging { get; set; }
    }

    public class Component
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("format")]
        public string? Format { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("example")]
        public Example? Example { get; set; }
    }

    public class TemplateDatum
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("parameter_format")]
        public string? ParameterFormat { get; set; }

        [JsonPropertyName("components")]
        public List<Component>? Components { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    public class Example
    {
        [JsonPropertyName("header_handle")]
        public List<string>? HeaderHandle { get; set; }

        [JsonPropertyName("body_text")]
        public List<List<string>>? BodyText { get; set; }
    }

    public class Paging
    {
        [JsonPropertyName("cursors")]
        public Cursors? Cursors { get; set; }
    }

    public class Cursors
    {
        [JsonPropertyName("before")]
        public string? Before { get; set; }

        [JsonPropertyName("after")]
        public string? After { get; set; }
    }
}