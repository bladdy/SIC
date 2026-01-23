using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class ResponseFromWhatsApp
    {
        public int Id { get; set; }
        public string MessageId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string From { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}