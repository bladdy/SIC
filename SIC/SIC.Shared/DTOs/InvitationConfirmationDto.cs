using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs;

public class InvitationConfirmationDto
{
    public string? CodigoInvitacion { get; set; }
    public string? Nombre { get; set; }
    public int CantidadDeMayores { get; set; }
    public int CantidadDeJovenes { get; set; }
    public int CantidadDeMenores { get; set; }
    public bool ConfirmacionAsistencia { get; set; }  // 'true' para Asistir, 'false' para No Asistir
    public int ConfirmadosAdultos { get; set; }
    public int confirmadosJovenes { get; set; }
    public int ConfirmadosMenores { get; set; }
    public string? Mensaje { get; set; }  // Mensaje personalizado (dedicatoria)
    public string? CodigoQR { get; set; } // Si es necesario, puede estar en la respuesta
}