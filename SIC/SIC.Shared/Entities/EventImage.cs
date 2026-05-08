using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class EventImage
{
    public int Id { get; set; }

    [Required]
    public string Url { get; set; } = null!;

    // Opcional: nombre físico del archivo
    public string? FileName { get; set; }

    // Tipo de Archivo audio, video, imagen, etc. para poder diferenciarlo en el album y mostrarlo de forma correcta
    public string? ImageType { get; set; }

    // Para ordenarlas en el álbum
    public DateTime PostingDate { get; set; } = DateTime.UtcNow;

    // 🔗 Relación con Event
    public int EventId { get; set; }

    public Event Event { get; set; } = null!;
}