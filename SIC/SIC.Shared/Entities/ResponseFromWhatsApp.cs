using System.ComponentModel.DataAnnotations.Schema;

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
        public string? NameConversation { get; set; }
        public string? EventCode { get; set; }
        public string? EventName { get; set; }
        public string? PhoneNumber { get; set; }

        /*
        /*
        //Para poder agrupar los mensajes enviados y recibidos por Evento y Usuario, hay que agregarles estos campos
        nombre: "Juan Pérez",
        Evento: "José & Maria",
        Codigo: "SG3HTU",
        */
    }
}