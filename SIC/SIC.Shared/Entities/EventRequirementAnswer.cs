using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities;

public class EventRequirementAnswer
{
    public int Id { get; set; }

    [Display(Name = "Evento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event? Event { get; set; }

    [Display(Name = "Requisito")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int RequirementId { get; set; }

    [ForeignKey("RequirementId")]
    public EventRequirement? Requirement { get; set; }

    [Display(Name = "Valor")]
    [MaxLength(2000, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Value { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EventRequirementImage> Images { get; set; } = new List<EventRequirementImage>();
}
