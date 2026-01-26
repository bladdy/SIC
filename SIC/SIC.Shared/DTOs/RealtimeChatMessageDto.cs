using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class RealtimeChatMessageDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string Direction { get; set; } = null!;
        public string MessageType { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}