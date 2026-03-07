using System;
using System.Collections.Generic;

namespace SIC.Shared.DTOs
{
    public class AdminDashboardDto
    {
        // 📊 Estadísticas generales
        public int EventsTotal { get; set; }

        public int EventsActive { get; set; }
        public int InvitationsTotal { get; set; }
        public int InvitationsConfirmed { get; set; }
        public int UsersTotal { get; set; }
        public int PlannersTotal { get; set; }
        public int MessagesSent { get; set; }
        public int MessagesFailed { get; set; }
        public int InvitationEntries { get; set; }

        // 👥 Estadísticas por tipo de usuario
        public int TotalAdmins { get; set; }

        public int TotalWeddingPlanners { get; set; }
        public int TotalRegularUsers { get; set; }
        public List<EventDashboardItemDto> UpcomingEvents { get; set; } = new();

        // 💳 Información de créditos de usuarios/planners
        public List<UserCreditDTO> UserCredits { get; set; } = new();

        // 🔝 Top 10 planners con menos créditos disponibles
        public List<UserCreditDTO> Top10PlannersLowCredits { get; set; } = new();

        // 📈 Top eventos (igual que PlannerDashboardDto)
        public List<EventDashboardItemDto> TopEvents { get; set; } = new();

        // 📅 Estadísticas de eventos recientes o próximos (opcional)
        public List<EventSummaryDto> RecentEvents { get; set; } = new();

        // 🔹 Totales de invitados globales
        public int TotalGuests { get; set; }             // Total de invitados (adultos + niños)

        public int ConfirmedGuests { get; set; }         // Invitados confirmados (adultos + niños)
        public int PendingGuests { get; set; }           // Invitados pendientes (adultos + niños)
        public int NotAttendingGuests { get; set; }      // Invitados que no asistirán (adultos + niños)
    }

    // 🔸 DTO reutilizado de PlannerDashboardDto
    public class EventDashboardItemDto
    {
        public string Code { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public int TotalGuests { get; set; }
        public int ConfirmedGuests { get; set; }
        public int PendingGuests { get; set; }
        public int NotAttendingGuests { get; set; }
        public DateTime Date { get; set; }
    }

    // 🔸 DTO simple para resumen de eventos (opcional)
    public class EventSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public string PlannerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int ConfirmedGuests { get; set; }
        public int TotalGuests { get; set; }
    }
}