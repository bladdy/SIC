using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities;

public class EventRequirementImage
{
    public int Id { get; set; }

    [Display(Name = "Respuesta del Requisito")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int RequirementAnswerId { get; set; }

    [ForeignKey("RequirementAnswerId")]
    public EventRequirementAnswer? RequirementAnswer { get; set; }

    [Display(Name = "Nombre del Archivo")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string FileName { get; set; } = null!;

    [Display(Name = "Nombre Original")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string OriginalName { get; set; } = null!;

    [Display(Name = "Ruta")]
    [MaxLength(500, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string Path { get; set; } = null!;

    [Display(Name = "Orden")]
    public int Order { get; set; }
}
