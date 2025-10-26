using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Enums;
using SIC.Shared.Response;

namespace SIC.Backend.Repositories.Implemetations;

public class DashboardReporsitory : IDashboardReporsitory
{
    private readonly DataContext _context;

    public DashboardReporsitory(DataContext context)
    {
        _context = context;
    }

    public async Task<ActionResponse<AdminDashboardDto>> GetAdminDashboardAsync(string adminUserId)
    {
        // 🧮 Estadísticas básicas (globales)
        var eventsTotal = await _context.Events.CountAsync();
        var eventsActive = await _context.Events.CountAsync(e => e.Status == Status.Active);
        var invitationsTotal = await _context.Invitations.CountAsync();
        var invitationsConfirmed = await _context.Invitations.CountAsync(i => i.Status == Status.Attend);
        var usersTotal = await _context.Users.CountAsync();
        var plannersTotal = await _context.Users.CountAsync(u => u.UserType == UserType.WeddingPlanner);
        var totalAdmins = await _context.Users.CountAsync(u => u.UserType == UserType.Admin);
        var totalRegularUsers = await _context.Users.CountAsync(u => u.UserType == UserType.User);
        var messagesSent = await _context.InvitationSendLogs.CountAsync();
        var messagesFailed = await _context.InvitationSendLogs.CountAsync(x => !x.IsSuccessful);
        var entries = await _context.InvitationEntries.CountAsync();

        // 👥 Créditos de todos los planners
        var userCredits = await _context.UserCredits
            .Select(c => new UserCreditDTO
            {
                Id = c.Id,
                UserId = c.UserId,
                FullName = c.User.FullName,
                TotalCredits = c.TotalCredits,
                AvailableCredits = c.AvailableCredits,
                ConsumedCredits = c.ConsumedCredits,
                PendingCredits = c.PendingCredits,
                LastUpdated = c.LastUpdated,
                UpdatedBy = c.UpdatedBy,
                Notes = c.Notes
            })
            .ToListAsync();

        // 🔝 Top 10 planners con menos créditos
        var top10PlannersLowCredits = userCredits
            .Where(c => c.AvailableCredits >= 0)
            .OrderBy(c => c.AvailableCredits)
            .Take(10)
            .ToList();

        // 📊 Totales de invitados filtrados por adminUserId
        var guestsStats = await _context.Events
            .Where(e => e.UserId == adminUserId)
            .Select(e => new
            {
                TotalGuests = e.Invitations.Sum(i => i.NumberAdults + i.NumberChildren),
                ConfirmedGuests = e.Invitations
                    .Where(i => i.Status == Status.Attend)
                    .Sum(i => i.NumberConfirmedAdults + i.NumberConfirmedChildren),
                PendingGuests = e.Invitations
                    .Where(i => i.Status == Status.Pending)
                    .Sum(i => i.NumberAdults + i.NumberChildren),
                NotAttendingGuests = e.Invitations
                    .Where(i => i.Status == Status.NotAttend)
                    .Sum(i => i.NumberAdults + i.NumberChildren)
            })
            .ToListAsync();

        var totalGuests = guestsStats.Sum(x => x.TotalGuests);
        var confirmedGuests = guestsStats.Sum(x => x.ConfirmedGuests);
        var pendingGuests = guestsStats.Sum(x => x.PendingGuests);
        var notAttendingGuests = guestsStats.Sum(x => x.NotAttendingGuests);

        // 🔝 Top 15 eventos (solo del admin actual)
        var topEventsRaw = await _context.Events
            .Where(e => e.UserId == adminUserId)
            .OrderByDescending(e => e.Invitations
                .Where(i => i.Status == Status.Attend)
                .Sum(i => i.NumberConfirmedAdults + i.NumberConfirmedChildren))
            .Take(15)
            .Select(e => new EventDashboardItemDto
            {
                EventName = e.Name,
                Date = e.Date,
                TotalGuests = e.Invitations.Sum(i => i.NumberAdults + i.NumberChildren),
                ConfirmedGuests = e.Invitations
                    .Where(i => i.Status == Status.Attend)
                    .Sum(i => i.NumberConfirmedAdults + i.NumberConfirmedChildren),
                PendingGuests = e.Invitations
                    .Where(i => i.Status == Status.Pending)
                    .Sum(i => i.NumberAdults + i.NumberChildren),
                NotAttendingGuests = e.Invitations
                    .Where(i => i.Status == Status.NotAttend)
                    .Sum(i => i.NumberAdults + i.NumberChildren)
            })
            .ToListAsync();

        // 📅 Próximos eventos (solo del admin actual)
        var upcomingEventsRaw = await _context.Events
            .Where(e => e.UserId == adminUserId && e.Status == Status.Active && e.Date >= DateTime.Now)
            .OrderBy(e => e.Date)
            .Take(15)
            .Select(e => new EventDashboardItemDto
            {
                EventName = e.Name,
                Date = e.Date,
                TotalGuests = e.Invitations.Sum(i => i.NumberAdults + i.NumberChildren),
                ConfirmedGuests = e.Invitations
                    .Where(i => i.Status == Status.Attend)
                    .Sum(i => i.NumberConfirmedAdults + i.NumberConfirmedChildren),
                PendingGuests = e.Invitations
                    .Where(i => i.Status == Status.Pending)
                    .Sum(i => i.NumberAdults + i.NumberChildren),
                NotAttendingGuests = e.Invitations
                    .Where(i => i.Status == Status.NotAttend)
                    .Sum(i => i.NumberAdults + i.NumberChildren)
            })
            .ToListAsync();

        // 🧩 Construcción del DTO final
        var dto = new AdminDashboardDto
        {
            // Datos base
            EventsTotal = eventsTotal,
            EventsActive = eventsActive,
            InvitationsTotal = invitationsTotal,
            InvitationsConfirmed = invitationsConfirmed,
            UsersTotal = usersTotal,
            PlannersTotal = plannersTotal,
            TotalAdmins = totalAdmins,
            TotalWeddingPlanners = plannersTotal,
            TotalRegularUsers = totalRegularUsers,
            MessagesSent = messagesSent,
            MessagesFailed = messagesFailed,
            InvitationEntries = entries,

            // Créditos
            UserCredits = userCredits,
            Top10PlannersLowCredits = top10PlannersLowCredits,

            // Invitados solo del admin actual
            TotalGuests = totalGuests,
            ConfirmedGuests = confirmedGuests,
            PendingGuests = pendingGuests,
            NotAttendingGuests = notAttendingGuests,

            // Top y próximos eventos
            TopEvents = topEventsRaw,
            UpcomingEvents = upcomingEventsRaw
        };

        return new ActionResponse<AdminDashboardDto>
        {
            Success = true,
            Result = dto
        };
    }

    public async Task<ActionResponse<PlannerDashboardDto>> GetPlannerDashboardAsync(string plannerUserId)
    {
        // Eventos del planner
        var myEventsQuery = _context.Events
                                    .Where(e => e.UserId == plannerUserId);

        var myEventsCount = await myEventsQuery.CountAsync();

        // 🔹 Próximos eventos con TotalGuests
        var upcomingEventsRaw = await myEventsQuery
                                    .Where(e => e.Date >= DateTime.Today)
                                    .OrderBy(e => e.Date)
                                    .Take(15)
                                    .Select(e => new
                                    {
                                        e.Id,
                                        e.Name,
                                        e.Code,
                                        e.Date,
                                        TotalGuests = e.Invitations.Sum(i => i.NumberAdults + i.NumberChildren)
                                    })
                                    .ToListAsync();

        // IDs de eventos del planner
        var eventIds = await myEventsQuery.Select(e => e.Id).ToListAsync();

        // 🔹 Totales de invitaciones
        var invitationsTotal = await _context.Invitations
                                             .Where(i => eventIds.Contains(i.EventId))
                                             .CountAsync();

        var invitationsConfirmed = await _context.Invitations
                                                 .Where(i => eventIds.Contains(i.EventId) && i.Status == Status.Attend)
                                                 .CountAsync();

        // 🔹 Top eventos por invitados
        var topEventsRaw = await _context.Events
            .Where(e => eventIds.Contains(e.Id))
            .OrderByDescending(e => e.Invitations
                .Where(i => i.Status == Status.Attend)
                .Sum(i => i.NumberConfirmedAdults + i.NumberConfirmedChildren))
            .Take(15)
            .Select(e => new
            {
                e.Id,
                e.Name,
                e.Code,
                TotalInvitations = e.Invitations.Count(),
                Confirmed = e.Invitations.Count(i => i.Status == Status.Attend),
                Pending = e.Invitations.Count(i => i.Status == Status.Pending),
                NotAttending = e.Invitations.Count(i => i.Status == Status.NotAttend),

                // 🔹 Invitados (adultos + niños)
                TotalGuests = e.Invitations.Sum(i => i.NumberAdults + i.NumberChildren),
                ConfirmedGuests = e.Invitations
                    .Where(i => i.Status == Status.Attend)
                    .Sum(i => i.NumberConfirmedAdults + i.NumberConfirmedChildren),
                PendingGuests = e.Invitations
                    .Where(i => i.Status == Status.Pending)
                    .Sum(i => i.NumberAdults + i.NumberChildren),
                NotAttendingGuests = e.Invitations
                    .Where(i => i.Status == Status.NotAttend)
                    .Sum(i => i.NumberAdults + i.NumberChildren)
            })
            .ToListAsync();

        // 🔹 Totales de invitados del planner
        var totalGuests = topEventsRaw.Sum(x => x.TotalGuests);
        var confirmedGuests = topEventsRaw.Sum(x => x.ConfirmedGuests);
        var pendingGuests = topEventsRaw.Sum(x => x.PendingGuests);
        var notAttendingGuests = topEventsRaw.Sum(x => x.NotAttendingGuests);

        // Construcción del DTO
        var dto = new PlannerDashboardDto
        {
            MyEventsCount = myEventsCount,

            UpcomingEvents = upcomingEventsRaw
                .Select(x => new PlannerDashboardDto.EventSummary
                {
                    EventId = x.Id,
                    Code = x.Code,
                    EventName = x.Name,
                    Date = x.Date,
                    TotalGuests = x.TotalGuests
                }).ToList(),

            MyInvitationsTotal = invitationsTotal,
            MyInvitationsConfirmed = invitationsConfirmed,

            TotalGuests = totalGuests,
            ConfirmedGuests = confirmedGuests,
            PendingGuests = pendingGuests,
            NotAttendingGuests = notAttendingGuests,

            TopEvents = topEventsRaw
                .Select(x => new PlannerDashboardDto.TopEvent
                {
                    EventId = x.Id,
                    EventName = x.Name,
                    Code = x.Code,
                    Confirmed = x.Confirmed,
                    Pending = x.Pending,
                    NotAttending = x.NotAttending,
                    TotalInvitations = x.TotalInvitations,

                    TotalGuests = x.TotalGuests,
                    ConfirmedGuests = x.ConfirmedGuests,
                    PendingGuests = x.PendingGuests,
                    NotAttendingGuests = x.NotAttendingGuests
                }).ToList()
        };

        return new ActionResponse<PlannerDashboardDto>
        {
            Success = true,
            Result = dto
        };
    }

    public async Task<ActionResponse<UserDashboardDto>> GetUserDashboardAsync(string userId)
    {
        var userEvent = await _context.Events
                                      .Where(e => e.UserId == userId)
                                      .OrderByDescending(e => e.Date)
                                      .Include(e => e.Invitations)
                                      .FirstOrDefaultAsync();

        if (userEvent == null)
        {
            return new ActionResponse<UserDashboardDto>
            {
                Success = true,
                Message = "No tienes eventos que mostrar."
            };
        }

        var totalInvitations = userEvent.Invitations.Count;
        var confirmed = userEvent.Invitations.Count(i => i.Status == Status.Attend);
        var pending = userEvent.Invitations.Count(i => i.Status == Status.Pending);
        var adultsConfirmed = userEvent.Invitations.Where(i => i.Status == Status.Attend).Sum(i => i.NumberConfirmedAdults);
        var childrenConfirmed = userEvent.Invitations.Where(i => i.Status == Status.Attend).Sum(i => i.NumberConfirmedChildren);

        var entries = await _context.InvitationEntries.CountAsync(x => x.EventId == userEvent.Id);

        var dto = new UserDashboardDto
        {
            EventId = userEvent.Id,
            EventName = userEvent.Name,
            EventDate = userEvent.Date,
            TotalInvitations = totalInvitations,
            Confirmed = confirmed,
            Pending = pending,
            AdultsConfirmed = adultsConfirmed,
            ChildrenConfirmed = childrenConfirmed,
            Entries = entries
        };

        return new ActionResponse<UserDashboardDto>
        {
            Success = true,
            Result = dto
        };
    }
}