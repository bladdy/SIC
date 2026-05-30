using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities
{
    public class UsuarioWhatsAppConfig
    {
        public int Id { get; set; }

        // 🔗 Relación con tu sistema
        [Required]
        public string UsuarioId { get; set; } = null!;

        public User? Usuario { get; set; }

        // 🔐 TOKEN PERMANENTE (encriptado en DB)
        [Required, MaxLength(500)]
        public string AccessToken { get; set; } = string.Empty;

        // 📱 WhatsApp
        [Required, MaxLength(100)]
        public string PhoneNumberId { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, MaxLength(100)]
        public string WabaId { get; set; } = null!;

        // 🏢 Meta Business
        [Required, MaxLength(100)]
        public string BusinessId { get; set; } = null!;

        [Required, MaxLength(100)]
        public string SystemUserId { get; set; } = null!;

        // 🔄 Estado
        public bool IsActive { get; set; } = true;

        // 🕒 Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }
        public DateTime TokenExpiresAt { get; set; }
    }
}