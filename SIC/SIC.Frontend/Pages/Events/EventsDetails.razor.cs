using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Collections.Generic;
using System.Net;

namespace SIC.Frontend.Pages.Events;

public partial class EventsDetails
{
    private int currentPage = 1;
    private int totalPages;

    public Event? EventDetail { get; set; }
    public List<Invitation>? Invitations { get; set; }
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public int RecordsNumber { get; set; } = 10;

    [Parameter] public string? Code { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadEvent();
        await LoadInvitations();
    }

    private async Task SelectedPageAsync(int page)
    {
        currentPage = page;
        await LoadInvitations(currentPage);
    }

    private async Task LoadInvitations(int page = 4)
    {
        if (!string.IsNullOrWhiteSpace(Page))
        {
            page = Convert.ToInt32(Page);
        }
        var ok = await LoadListAsync(page);
        if (ok)
        {
            await LoadPagesAsync();
        }
    }

    private async Task CleanFilterAsync()
    {
        Filter = string.Empty;
        await ApplyFilterAsync();
    }

    private async Task ApplyFilterAsync()
    {
        int page = 1;
        await LoadInvitations(page);
        await SelectedPageAsync(page);
    }

    private async Task<bool> LoadListAsync(int page)
    {
        var url = $"api/Invitations/paginated?Id={EventDetail!.Id}&PageNumber={page}&RecordsNumber={RecordsNumber}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }

        var responseHttp = await Repository.GetAsync<List<Invitation>>(url);

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo("/events");
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
        }

        Invitations = responseHttp?.Response ?? new List<Invitation>();
        return true;
    }

    private async Task LoadPagesAsync()
    {
        var url = $"api/Invitations/totalRecords?Id={EventDetail!.Id}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }
        var responseHttp = await Repository.GetAsync<int>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        // ⚡ Backend ya devuelve total de páginas, no de registros
        totalPages = responseHttp.Response;
    }

    private async Task LoadEvent()
    {
        var responseHttp = await Repository.GetAsync<Event>($"api/Events/byCode/{Code}");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo("/events");
                return;
            }
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        EventDetail = responseHttp?.Response;
    }

    private async Task FilterCallBack(string filter)
    {
        Filter = filter;
        await ApplyFilterAsync();
        StateHasChanged();

        Filter = filter;
    }
}