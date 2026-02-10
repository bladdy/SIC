using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class MetaToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Token
        public string AccessToken { get; set; } = null!;

        // Tipo (bearer)
        public string TokenType { get; set; } = "bearer";

        // Segundos de expiración (0 o null = permanente)
        public int? ExpiresIn { get; set; }

        // Scopes concedidos
        public string? Scopes { get; set; }

        // Relación con negocio / cuenta
        public string? BusinessId { get; set; }

        public string? SystemUserId { get; set; }

        // Auditoría
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt =>
            ExpiresIn.HasValue
                ? CreatedAt.AddSeconds(ExpiresIn.Value)
                : null;

        public bool IsPermanent => !ExpiresIn.HasValue || ExpiresIn == 0;
    }
}