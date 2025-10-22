using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Shared;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.RegisterEvent;

public partial class InvitationsEntriesIndex
{
    private int currentPage = 1;
    private int totalPages;
    [Parameter] public string? Code { get; set; }
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }
    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public string OrderBy { get; set; } = "";
    [Inject] private IRepository Repository { get; set; } = default!;
    public List<InvitationEntry>? InvitationEntries { get; set; }
    private InvitationEntry NewInvitationEntry = new();
    private bool IsModalVisible = false;
    private bool IsEditMode = false;

    protected override async Task OnInitializedAsync()
    {
        // Si RecordsNumber viene null, lo asignamos a 15
        RecordsNumber ??= 15;

        await base.OnInitializedAsync();
        await LoadInvitationEntries(currentPage);
    }

    private async Task LoadInvitationEntries(int page = 1)
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

    private async Task<bool> LoadPagesAsync()
    {
        var url = $"api/InvitationEntry/paginated?Code={Code}&RecordsNumber={RecordsNumber}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }

        if (!string.IsNullOrWhiteSpace(OrderBy))
        {
            url += $"&OrderBy={OrderBy}";
        }
        var responseHttp = await Repository.GetAsync<List<InvitationEntry>>(url);

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

        InvitationEntries = responseHttp?.Response ?? new List<InvitationEntry>();
        return true;
    }

    private async Task<bool> LoadListAsync(int page)
    {
        var url = $"api/InvitationEntry/paginated?Code={Code}&PageNumber={page}&PageSize={RecordsNumber}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }

        if (!string.IsNullOrWhiteSpace(OrderBy))
        {
            url += $"&OrderBy={OrderBy}";
        }
        var responseHttp = await Repository.GetAsync<List<InvitationEntry>>(url);

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo($"/events/details/{Code}");
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
        }

        InvitationEntries = responseHttp?.Response ?? new List<InvitationEntry>();
        return true;
    }

    private async Task ApplyFilterAsync()
    {
        int page = 1;
        await LoadInvitationEntries(page);
        await SelectedPageAsync(page);
    }

    private async Task SelectedPageAsync(int page)
    {
        currentPage = page;
        await LoadInvitationEntries(currentPage);
    }

    private async Task CleanFilterAsync()
    {
        Filter = string.Empty;
        await ApplyFilterAsync();
    }
}