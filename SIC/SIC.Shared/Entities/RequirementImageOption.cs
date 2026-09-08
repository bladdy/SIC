using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities;

public class RequirementImageOption
{
    public int Id { get; set; }

    [Display(Name = "Requisito")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int RequirementId { get; set; }

    [ForeignKey("RequirementId")]
    public EventRequirement? Requirement { get; set; }

    [Display(Name = "Título")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Title { get; set; } = null!;

    [Display(Name = "URL de Imagen")]
    [MaxLength(500, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string ImageUrl { get; set; } = null!;

    [Display(Name = "Orden")]
    public int SortOrder { get; set; }
}
