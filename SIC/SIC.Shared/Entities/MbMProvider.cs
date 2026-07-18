using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class MbMProvider
{
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;

    [Display(Name = "Contacto")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Contact { get; set; }

    [Display(Name = "Servicio")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    public string? Service { get; set; }

    [Display(Name = "Estado")]
    public ProviderStatus Status { get; set; } = ProviderStatus.Confirmado;

    [Display(Name = "Costo")]
    [Range(0, double.MaxValue, ErrorMessage = "El campo {0} debe ser un valor positivo.")]
    public decimal? Cost { get; set; }

    public int MbMActivityId { get; set; }
    public MbMActivity? MbMActivity { get; set; }
}
