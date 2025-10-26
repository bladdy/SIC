using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class AddCreditsRequest
    {
        public string UserId { get; set; } = null!;
        public int CreditsToAdd { get; set; }
        public string? Notes { get; set; }
        public string? UpdatedBy { get; set; }
    }
}