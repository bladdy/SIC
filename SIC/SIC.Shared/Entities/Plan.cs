
using SIC.Shared.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class Plan : IEntityWithName
{
    public int Id { get; set; }
    [Display(Name = "Nombre")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;
    public decimal Price { get; set; } = 0;
    public ICollection<PlanItem>? PlanItems { get; set; }
    [Display(Name = "N° Servicios")]
    public int ItemsNumbers => PlanItems == null || PlanItems.Count == 0 ? 0 : PlanItems.Count;
}
