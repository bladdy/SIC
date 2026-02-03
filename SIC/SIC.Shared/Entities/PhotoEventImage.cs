using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities
{
    public class PhotoEventImage
    {
        public int Id { get; set; }
        public string? Code { get; set; }

        [Required]
        public string Url { get; set; } = null!;

        // Opcional: nombre físico del archivo
        public string? FileName { get; set; }

        // Para ordenarlas en el álbum
        public DateTime PostingDate { get; set; } = DateTime.UtcNow;

        // 🔗 Relación con Event
        public int PhotoEventId { get; set; }

        public PhotoEvent Event { get; set; } = null!;
    }
}