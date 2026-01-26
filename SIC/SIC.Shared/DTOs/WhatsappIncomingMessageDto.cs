using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class WhatsappIncomingMessageDto
    {
        public string? MessageId { get; set; }

        public string From { get; set; } = null!;

        public string? Text { get; set; }

        public string? Type { get; set; }
        public DateTime Timestamp { get; set; }

        public string? ReplyToMessageId { get; set; }

        public string? Status { get; set; } // sent | delivered | read | null

        public string Direction { get; set; } = null!; // IN / OUT
    }
}