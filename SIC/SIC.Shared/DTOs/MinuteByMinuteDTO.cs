using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class MinuteByMinuteDTO
{
    [Display(Name = "Título")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Title { get; set; } = null!;

    [Display(Name = "Público")]
    public bool IsPublic { get; set; } = false;
}
