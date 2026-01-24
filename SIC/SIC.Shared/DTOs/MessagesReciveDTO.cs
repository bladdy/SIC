using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class MessagesReciveDTO
    {
        public string InvitationName { get; set; } = null!;
        public string InvitationCode { get; set; } = null!;
        public string EventName { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string From { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

}