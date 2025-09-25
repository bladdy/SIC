using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class Item
{
    public int Id { get; set; }
    [Display(Name= "Nombre")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;
}