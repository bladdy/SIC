using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class BoletaInvitacionDto
    {
        public string NombreInvitado { get; set; } = string.Empty;
        public string NombreEvento { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Hora { get; set; } = string.Empty;
        public string Lugar { get; set; } = string.Empty;
        public int CantidadPersonas { get; set; }
        public string MesaAsignada { get; set; } = string.Empty;
        public string CodigoQr { get; set; } = string.Empty;
    }
}