using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class UserCreditHistoryDTO
    {
        public string ActionType { get; set; } = null!;
        public int Amount { get; set; }
        public int AvailableAfter { get; set; }
        public DateTime Date { get; set; }
        public string? ModifiedBy { get; set; }
        public string? Notes { get; set; }
    }
}