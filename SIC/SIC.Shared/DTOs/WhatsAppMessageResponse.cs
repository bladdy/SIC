using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class WhatsAppMessageResponse
    {
        public string MessageId { get; set; } = string.Empty;
        public string NumeroDestino { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
    }
}