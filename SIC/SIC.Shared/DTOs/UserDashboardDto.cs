using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class UserDashboardDto
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public int TotalInvitations { get; set; }
        public int Confirmed { get; set; }
        public int Pending { get; set; }
        public int AdultsConfirmed { get; set; }
        public int ChildrenConfirmed { get; set; }
        public int YoungConfirmed { get; set; }
        public int Entries { get; set; }
    }
}