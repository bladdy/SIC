using SIC.Shared.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class UserDTO : User
    {
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Contraseña")]
        [StringLength(20, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caractéres.", MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [Compare("Password", ErrorMessage = "La contraseña y la confirmacion no son iguales.")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Display(Name = "Confirmacion Contraseña")]
        [StringLength(20, ErrorMessage = "El campo {0} debe tener entre {2} y {1} caractéres.", MinimumLength = 6)]
        public string PasswordConfirm { get; set; } = null!;
    }
}