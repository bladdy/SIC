using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class InboxConversationDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string LastMessage { get; set; } = null!;
        public DateTime LastMessageAt { get; set; }
        public string Direction { get; set; } = null!;
        public string? Type { get; set; }
        public int UnreadCount { get; set; }
        public string? EventCode { get; set; }
        public string? EventName { get; set; }
        public string? NameConversation { get; set; }
    }
}