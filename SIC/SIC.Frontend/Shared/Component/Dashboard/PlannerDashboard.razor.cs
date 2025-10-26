using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using System.Net;

namespace SIC.Frontend.Shared.Component.Dashboard
{
    public partial class PlannerDashboard
    {
        private string? _userId;
        private int topEventsPage = 0;
        private int upcomingEventsPage = 0;
        private int pageSize = 5;
        [Inject] private IRepository Repository { get; set; } = default!;

        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        public PlannerDashboardDto? PlannerDashboards { get; set; }
        public UserCreditDTO? UserCreditDTO { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            await LoadDashboard();
            if (user.Identity?.IsAuthenticated ?? false)
            {
                _userId = user.Claims.FirstOrDefault(c => c.Type.EndsWith("nameidentifier"))?.Value;
                await LoadAvailableCreditsAsync();
            }
        }

        private async Task LoadDashboard()
        {
            var responseHttp = await Repository.GetAsync<PlannerDashboardDto>($"api/Dashboard/planner");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    return;
                }
                var message = await responseHttp.GetErrorMessageAsync();
                return;
            }
            PlannerDashboards = responseHttp?.Response;
        }

        private async Task LoadAvailableCreditsAsync()
        {
            try
            {
                var responseHttp = await Repository.GetAsync<UserCreditDTO>($"api/UserCredits/{_userId}") ?? null;
                UserCreditDTO = responseHttp?.Response;
            }
            catch
            {
                UserCreditDTO = null;
            }
        }
        private IEnumerable<PlannerDashboardDto.TopEvent> TopEventsPage =>
        PlannerDashboards.TopEvents.Skip(topEventsPage * pageSize).Take(pageSize);

        private IEnumerable<PlannerDashboardDto.EventSummary> UpcomingEventsPage =>
            PlannerDashboards.UpcomingEvents.Skip(upcomingEventsPage * pageSize).Take(pageSize);

        private int TopEventsTotalPages => (int)Math.Ceiling((double)PlannerDashboards.TopEvents.Count / pageSize);
        private int UpcomingEventsTotalPages => (int)Math.Ceiling((double)PlannerDashboards.UpcomingEvents.Count / pageSize);

        private void NextTopEvents() { if (topEventsPage < TopEventsTotalPages - 1) topEventsPage++; }
        private void PrevTopEvents() { if (topEventsPage > 0) topEventsPage--; }

        private void NextUpcomingEvents() { if (upcomingEventsPage < UpcomingEventsTotalPages - 1) upcomingEventsPage++; }
        private void PrevUpcomingEvents() { if (upcomingEventsPage > 0) upcomingEventsPage--; }
    }
}