using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class UsuarioWhatsAppConfig
    {
        public int Id { get; set; }

        [Required]
        public string? UsuarioId { get; set; }   // Relación con tu entidad Usuario

        public User? Usuario { get; set; }

        [Required, MaxLength(500)]
        public string AccessToken { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string PhoneNumberId { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; } // Opcional: para mostrar el número en la UI
    }
}