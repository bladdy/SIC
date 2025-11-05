using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class PlannerDashboardDto
    {
        public int MyEventsCount { get; set; }
        public int MyInvitationsTotal { get; set; }
        public int MyInvitationsConfirmed { get; set; }
        public int TotalGuests { get; set; }
        public int ConfirmedGuests { get; set; }
        public int PendingGuests { get; set; }
        public int NotAttendingGuests { get; set; }
        public List<EventSummary> UpcomingEvents { get; set; } = new();
        public List<TopEvent> TopEvents { get; set; } = new();

        public class EventSummary
        {
            public int EventId { get; set; }
            public string? Code { get; set; }
            public string EventName { get; set; } = string.Empty;
            public int TotalGuests { get; set; }
            public DateTime Date { get; set; }
        }

        public class TopEvent
        {
            public int EventId { get; set; }
            public string EventName { get; set; } = string.Empty;

            public string? Code { get; set; }

            public int Confirmed { get; set; }      // Invitaciones confirmadas
            public int Pending { get; set; }        // Invitaciones pendientes
            public int NotAttending { get; set; }   // Invitaciones no asistirán
            public int TotalInvitations { get; set; }

            // 🔹 Nuevos campos:
            public int TotalGuests { get; set; }             // Total adultos + niños

            public int ConfirmedGuests { get; set; }         // Confirmados (adultos + niños)
            public int PendingGuests { get; set; }           // Pendientes (adultos + niños)
            public int NotAttendingGuests { get; set; }      // No asistirán (adultos + niños)
            public DateTime Date { get; set; }

        }
    }
}