using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities;

public class EventTypeRequirement
{
    public int Id { get; set; }

    [Display(Name = "Tipo de Evento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int EventTypeId { get; set; }

    [ForeignKey("EventTypeId")]
    public EventType? EventType { get; set; }

    [Display(Name = "Requisito")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int RequirementId { get; set; }

    [ForeignKey("RequirementId")]
    public EventRequirement? Requirement { get; set; }

    [Display(Name = "Orden")]
    public int SortOrder { get; set; }
}
