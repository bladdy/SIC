using SIC.Shared.Enums;
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

    [Display(Name = "Asignado a")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? AssignedTo { get; set; }

    [Display(Name = "Fecha Límite")]
    public DateTime? DueDate { get; set; }

    [Display(Name = "Prioridad")]
    public ActivityPriority Priority { get; set; } = ActivityPriority.Media;

    public int MbMActivityId { get; set; }
    public MbMActivity? MbMActivity { get; set; }
}
