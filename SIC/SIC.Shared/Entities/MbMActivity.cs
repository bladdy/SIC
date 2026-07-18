using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class MbMActivity
{
    public int Id { get; set; }

    [Display(Name = "Título")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Title { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(1000, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Description { get; set; }

    [Display(Name = "Hora de Inicio")]
    public DateTime StartTime { get; set; }

    [Display(Name = "Hora de Fin")]
    public DateTime? EndTime { get; set; }

    [Display(Name = "Estado")]
    public ActivityStatus Status { get; set; } = ActivityStatus.Pendiente;

    [Display(Name = "Prioridad")]
    public ActivityPriority Priority { get; set; } = ActivityPriority.Media;

    [Display(Name = "Ubicación")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Location { get; set; }

    [Display(Name = "Notas")]
    [MaxLength(2000, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Notes { get; set; }

    public int MinuteByMinuteId { get; set; }
    public MinuteByMinute? MinuteByMinute { get; set; }

    public ICollection<MbMProvider> Providers { get; set; } = new List<MbMProvider>();
    public ICollection<MbMTask> Tasks { get; set; } = new List<MbMTask>();
}
