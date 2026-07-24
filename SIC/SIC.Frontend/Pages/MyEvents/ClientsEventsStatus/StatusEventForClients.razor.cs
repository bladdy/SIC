using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;
using System.Net;

namespace SIC.Frontend.Pages.MyEvents.ClientsEventsStatus
{
    public partial class StatusEventForClients
    {
        private int currentPage = 1;
        private int totalPages;
        public Event? EventDetail { get; set; }
        public List<Invitation>? Invitations { get; set; }
        public List<WhatsAppTemplate>? Templates { get; set; }
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public int RecordsNumber { get; set; } = 50;

        [Parameter] public string? Code { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!string.IsNullOrWhiteSpace(Page) && int.TryParse(Page, out var pageFromQuery))
            {
                currentPage = pageFromQuery;
            }
            await LoadEvent();
            await LoadInvitations();
            await LoadTemplates();
        }

        private async Task LoadTemplates()
        {
            var url = $"api/whatsapp/get-templates";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&Filter={Filter}";
            }

            //.GetAsync<List<Invitation>>
            var responseHttp = await Repository.GetAsync<List<WhatsAppTemplate>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            // Backend ya devuelve total de páginas, no de registros
            Templates = responseHttp.Response ?? new List<WhatsAppTemplate>();
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadInvitations(currentPage);
        }

        private async Task LoadInvitations(int page = 1)
        {
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
            var url = $"api/Invitations/paginated?Id={EventDetail!.Id}&PageNumber={page}&RecordsNumber={(RecordsNumber == 0 ? 50 : RecordsNumber)}";

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

            // Backend ya devuelve total de páginas, no de registros
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

        private string GetStatusBadge(Status status) => status switch
        {
            Status.Attend => "success",
            Status.NotAttend => "danger",
            _ => "secondary"
        };
    }
}