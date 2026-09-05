using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class RecoverPasswordDTO
{
    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Phone(ErrorMessage = "El campo {0} debe ser un teléfono válido.")]
    public string PhoneNumber { get; set; } = null!;
}