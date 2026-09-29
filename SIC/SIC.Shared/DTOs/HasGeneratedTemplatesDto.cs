using System.Text.Json.Serialization;

namespace SIC.Shared.DTOs;

public class HasGeneratedTemplatesDto
{
    [JsonPropertyName("hasGenerated")]
    public bool HasGenerated { get; set; }
}