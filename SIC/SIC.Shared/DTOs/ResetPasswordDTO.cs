using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class ResetPasswordDTO
{
    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [Phone(ErrorMessage = "El campo {0} debe ser un teléfono válido.")]
    public string PhoneNumber { get; set; } = null!;

    [Display(Name = "Token")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Token { get; set; } = null!;

    [Display(Name = "Nueva Contraseña")]
    [DataType(DataType.Password)]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [StringLength(20, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caractéres.", MinimumLength = 6)]
    public string NewPassword { get; set; } = null!;
}