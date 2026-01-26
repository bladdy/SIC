using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class SendWhatsappMessageDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}