using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class WhatsappInboxItemDto
    {
        public string PhoneNumber { get; set; } = null!;
        public string LastMessage { get; set; } = null!;
        public DateTime LastDate { get; set; }
        public int UnreadCount { get; set; }
    }
}