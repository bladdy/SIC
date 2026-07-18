using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class MbMProviderDTO
{
    [Display(Name = "Nombre")]
    [MaxLength(200, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;

    [Display(Name = "Contacto")]
    [MaxLength(100)]
    public string? Contact { get; set; }

    [Display(Name = "Servicio")]
    [MaxLength(200)]
    public string? Service { get; set; }

    [Display(Name = "Estado")]
    public ProviderStatus Status { get; set; } = ProviderStatus.Confirmado;

    [Display(Name = "Costo")]
    [Range(0, double.MaxValue)]
    public decimal? Cost { get; set; }
}
