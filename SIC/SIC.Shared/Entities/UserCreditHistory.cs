using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities
{
    public class UserCreditHistory
    {
        public int Id { get; set; }

        [Required]
        public int UserCreditId { get; set; }

        [ForeignKey(nameof(UserCreditId))]
        public UserCredit UserCredit { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Display(Name = "Tipo de acción")]
        public string ActionType { get; set; } = null!; // "Carga", "Consumo", "Ajuste"

        [Display(Name = "Cantidad de créditos")]
        public int Amount { get; set; }

        [Display(Name = "Créditos disponibles después de la acción")]
        public int AvailableAfter { get; set; }

        [Display(Name = "Fecha")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        [Display(Name = "Modificado por")]
        public string? ModifiedBy { get; set; }

        [MaxLength(250)]
        [Display(Name = "Notas o motivo")]
        public string? Notes { get; set; }
    }
}