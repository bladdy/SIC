using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class WhatsAppAccount
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Meta / Business
        public string BusinessId { get; set; } = null!;

        public string WabaId { get; set; } = null!;
        public string PhoneNumberId { get; set; } = null!;

        // System User
        public string SystemUserId { get; set; } = null!;

        // Token permanente (usar SIEMPRE)
        public string PermanentAccessToken { get; set; } = null!;

        // Estado
        public bool IsActive { get; set; } = true;

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        // Opcional (multi-tenant)
        public string? TenantId { get; set; }
    }
}