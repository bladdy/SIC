using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class MessageWhatsappInvitationDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string MessageConfirmation { get; set; } = null!;
        public string MessageInvitation { get; set; } = null!;
        public bool Sent { get; set; } = false;
        public string Error { get; set; } = "";
    }
}