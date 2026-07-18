using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class MinuteByMinute
{
    public int Id { get; set; }

    [Display(Name = "Título")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Title { get; set; } = null!;

    [Display(Name = "Público")]
    public bool IsPublic { get; set; } = false;

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public ICollection<MbMActivity> Activities { get; set; } = new List<MbMActivity>();
}
