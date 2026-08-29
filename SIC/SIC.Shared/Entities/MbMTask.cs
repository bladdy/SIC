using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class MbMTask
{
    public int Id { get; set; }

    [Display(Name = "Título")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Title { get; set; } = null!;

    [Display(Name = "Completada")]
    public bool IsCompleted { get; set; } = false;

    [Display(Name = "Proveedor/Responsable")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? AssignedTo { get; set; }

    [Display(Name = "Teléfono")]
    [MaxLength(30, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? ResponsiblePhone { get; set; }

    [Display(Name = "Movito")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Motivo { get; set; }

    public int MbMActivityId { get; set; }
    public MbMActivity? MbMActivity { get; set; }
}