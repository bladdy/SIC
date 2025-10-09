using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class MassiveShippingProgress
    {
        public int Id { get; set; }
        public int EventoId { get; set; }
        public int UltimaTandaEnviada { get; set; }
        public int TotalTandas { get; set; }
        public DateTime FechaUltimoEnvio { get; set; }
        public bool Completado { get; set; }
    }
}