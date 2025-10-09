using SIC.Shared.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities
{
    public class Message : IEntityWithTitle
    {
        public int Id { get; set; }

        [Display(Name = "Titulo")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Title { get; set; } = null!;

        [Display(Name = "Subtitulo")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string SubTitle { get; set; } = null!;

        [Display(Name = "Mensaje de invitación")]
        public string MessageInvitation { get; set; } = null!;

        [Display(Name = "Mensaje de Confirmación")]
        public string MessageConfirmation { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; } = null!;
    }
}