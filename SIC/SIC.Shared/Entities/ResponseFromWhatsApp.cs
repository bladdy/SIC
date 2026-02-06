using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class ResponseFromWhatsApp
    {
        public int Id { get; set; }
        public string MessageId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string From { get; set; } = null!;//Número WhatsApp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Status { get; set; }
        public string Direction { get; set; } = null!; // IN / OUT
        public string? Type { get; set; }// text, template, image

        /*
        //Para poder agrupar los mensajes enviados y recibidos por Evento y Usuario, hay que agregarles estos campos
        nombre: "Juan Pérez",
        Evento: "José & Maria",
        Codigo: "SG3HTU",
        */
    }
}