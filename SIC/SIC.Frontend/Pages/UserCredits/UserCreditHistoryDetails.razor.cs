using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Pages.Events;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;
using System.Security.Claims;

namespace SIC.Frontend.Pages.UserCredits
{
    public partial class UserCreditHistoryDetails
    {
        private int totalPages;
        private int currentPage = 1;
        public List<UserCreditHistory>? userCreditHistories { get; set; }
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!; 
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }
        [Parameter] public string? Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;
                if (user.Identity is not null && user.Identity.IsAuthenticated)
                {
                    Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                }
                
            }
            RecordsNumber ??= 15;
            await LoadCreditHistoriy();
        }

        private async Task SelectedPage(int page)
        {
            currentPage = page;
            await LoadCreditHistoriy(page);
        }

        private async Task LoadCreditHistoriy(int page = 1)
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

        private async Task LoadPagesAsync()
        {//https://localhost:7141/api/UserCredits/paginated?UserId=2c71487c-5df3-429e-b527-b1d9f9b4a241
            var url = $"api/UserCredits/totalRecords?UserId={Id}&RecordsNumber={RecordsNumber}";

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

        private async Task<bool> LoadListAsync(int page)
        {
            var url = $"api/UserCredits/paginated?UserId={Id}&PageNumber={page}&PageSize={RecordsNumber}";
            var responseHttp = await Repository.GetAsync<List<UserCreditHistory>>(url);

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

            userCreditHistories = responseHttp?.Response ?? new List<UserCreditHistory>();
            return true;
        }
    }
}