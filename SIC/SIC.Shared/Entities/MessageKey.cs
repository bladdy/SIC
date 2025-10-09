using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class MessageKey
    {
        [Key]
        public int Id { get; set; } // Identificador único para cada clave

        [Required]
        [MaxLength(500)]
        public string Key { get; set; } // La clave del mensaje

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } // Descripción de la clave

        [Required]
        [MaxLength(100)]
        public string PropertyName { get; set; } // Nombre de la propiedad de Invitation, ej. "Name"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Fecha de creación
        public DateTime? UpdatedAt { get; set; } // Fecha de última actualización (opcional)
    }
}