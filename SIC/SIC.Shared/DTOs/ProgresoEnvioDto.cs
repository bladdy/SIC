using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class ProgresoEnvioDto
    {
        public string Evento { get; set; } = string.Empty;
        public int TotalInvitaciones { get; set; }
        public int TotalEnviadas { get; set; }
        public int UltimaTanda { get; set; }
        public int TotalTandas { get; set; }
        public int? SiguienteTanda { get; set; }
        public bool Completado { get; set; }
    }
}