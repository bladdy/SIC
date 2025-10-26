using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities;

public class UserCredit
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Display(Name = "Créditos totales asignados")]
    [Range(0, int.MaxValue)]
    public int TotalCredits { get; set; } = 0;

    [Display(Name = "Créditos disponibles")]
    [Range(0, int.MaxValue)]
    public int AvailableCredits { get; set; } = 0;

    [Display(Name = "Créditos consumidos")]
    [Range(0, int.MaxValue)]
    public int ConsumedCredits { get; set; } = 0;

    [Display(Name = "Créditos pendientes por agregar")]
    [Range(0, int.MaxValue)]
    public int PendingCredits { get; set; } = 0;

    [Display(Name = "Última actualización")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [MaxLength(100)]
    [Display(Name = "Modificado por")]
    public string? UpdatedBy { get; set; }

    [MaxLength(250)]
    [Display(Name = "Notas o motivo de cambio")]
    public string? Notes { get; set; }

    [NotMapped]
    public string Summary => $"{AvailableCredits}/{TotalCredits}";

    public ICollection<UserCreditHistory>? CreditHistory { get; set; }
}