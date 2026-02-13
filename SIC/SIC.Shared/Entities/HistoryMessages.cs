using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class HistoryMessages
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;
        public int InvitationId { get; set; }
        public Invitation Invitation { get; set; } = null!;
        public DateTime SendDate { get; set; }
        public bool Send { get; set; }
        public bool Delivered { get; set; }
        public bool Error { get; set; }
        public string? Message { get; set; }
    }
}