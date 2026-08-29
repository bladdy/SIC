using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs
{
    public class ChangePasswordDTO
    {
        [Display(Name = "Contraseña actual")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = null!;

        [Display(Name = "Contraseña nueva")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "El campo {0} debe tener al menos {1} caracteres.")]
        public string NewPassword { get; set; } = null!;
    }
}
