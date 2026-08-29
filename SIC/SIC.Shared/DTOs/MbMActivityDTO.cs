using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class MbMActivityDTO
{
    [Display(Name = "Título")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Title { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Responsable")]
    [MaxLength(200)]
    public string? Responsible { get; set; }

    [Display(Name = "Tipo / Cargo")]
    [MaxLength(200)]
    public string? ResponsibleRole { get; set; }

    [Display(Name = "Número de Responsable")]
    [MaxLength(30)]
    public string? ResponsiblePhone { get; set; }

    [Display(Name = "Fecha y Hora")]
    public DateTime StartTime { get; set; }

    [Display(Name = "Estado")]
    public ActivityStatus Status { get; set; } = ActivityStatus.Pendiente;

    [Display(Name = "Ubicación")]
    [MaxLength(200)]
    public string? Location { get; set; }

    [Display(Name = "Notas")]
    [MaxLength(2000)]
    public string? Notes { get; set; }
}
