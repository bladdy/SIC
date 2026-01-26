using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class ChatMessageDto
    {
        public string Direction { get; set; } = null!;
        public string? Content { get; set; }
        public string MessageType { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}