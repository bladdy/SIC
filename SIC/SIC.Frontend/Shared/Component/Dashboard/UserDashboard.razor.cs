using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using System.Net;

namespace SIC.Frontend.Shared.Component.Dashboard;

public partial class UserDashboard
{
    [Inject] private IRepository Repository { get; set; } = default!;
    public UserDashboardDto? UserDashboards { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboard();
    }

    private async Task LoadDashboard()
    {
        var responseHttp = await Repository.GetAsync<UserDashboardDto>($"api/Dashboard/user");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
            var message = await responseHttp.GetErrorMessageAsync();
            return;
        }
        UserDashboards = responseHttp?.Response;
    }
}