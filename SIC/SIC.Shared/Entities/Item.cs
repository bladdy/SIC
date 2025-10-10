using SIC.Shared.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class Item : IEntityWithName
{
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;

    //public double Price { get; set; } 1 y 2

    //public double Price { get; set; } 1 y 2
    public ICollection<PlanItem>? PlanItems { get; set; }
}