using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Pages.Events;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Shared.Component.Dashboard;

public partial class AdminDashboard
{
    //ToDo: Agregar el consumo cuando sea un planner para que haga el descuento de los creditos
    [Inject] private IRepository Repository { get; set; } = default!;
    public AdminDashboardDto? AdminDashboards { get; set; }

    private int topEventsPage = 0;
    private int upcomingEventsPage = 0;
    private int pageSize = 5;

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboard();
    }

    private async Task LoadDashboard()
    {
        var responseHttp = await Repository.GetAsync<AdminDashboardDto>("api/Dashboard/admin");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            var message = await responseHttp.GetErrorMessageAsync();
            return;
        }
        AdminDashboards = responseHttp?.Response;
    }

    private IEnumerable<EventDashboardItemDto> TopEventsPage =>
    AdminDashboards.TopEvents.Skip(topEventsPage * pageSize).Take(pageSize);

    private IEnumerable<EventDashboardItemDto> UpcomingEventsPage =>
        AdminDashboards.UpcomingEvents.Skip(upcomingEventsPage * pageSize).Take(pageSize);

    private int TopEventsTotalPages => (int)Math.Ceiling((double)AdminDashboards.TopEvents.Count / pageSize);
    private int UpcomingEventsTotalPages => (int)Math.Ceiling((double)AdminDashboards.UpcomingEvents.Count / pageSize);

    private void NextTopEvents()
    { if (topEventsPage < TopEventsTotalPages - 1) topEventsPage++; }

    private void PrevTopEvents()
    { if (topEventsPage > 0) topEventsPage--; }

    private void NextUpcomingEvents()
    { if (upcomingEventsPage < UpcomingEventsTotalPages - 1) upcomingEventsPage++; }

    private void PrevUpcomingEvents()
    { if (upcomingEventsPage > 0) upcomingEventsPage--; }
}