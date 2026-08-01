using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class EventRequirement
{
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Description { get; set; }

    [Display(Name = "Sección")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Section { get; set; } = null!;

    [Display(Name = "Tipo de Entrada")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public RequirementInputType InputType { get; set; }

    [Display(Name = "Placeholder")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Placeholder { get; set; }

    [Display(Name = "Obligatorio")]
    public bool IsRequired { get; set; }

    [Display(Name = "Mínimo de Imágenes")]
    [Range(0, 100)]
    public int MinImages { get; set; }

    [Display(Name = "Máximo de Imágenes")]
    [Range(0, 100)]
    public int MaxImages { get; set; }

    [Display(Name = "Orden")]
    public int SortOrder { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EventTypeRequirement> EventTypeRequirements { get; set; } = new List<EventTypeRequirement>();
    public ICollection<EventRequirementAnswer> Answers { get; set; } = new List<EventRequirementAnswer>();
}
