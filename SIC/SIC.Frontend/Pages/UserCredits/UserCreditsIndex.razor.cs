using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Frontend.Shared.Component.Modals;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.UserCredits
{
    public partial class UserCreditsIndex
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        public List<UserCreditDTO>? UserCredits { get; set; }
        private int currentPage = 1;
        private int totalPages;

        private AddCreditModal addCreditModal;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync(int page = 1)
        {
            await LoadListAsync(page);
        }

        private async Task SelectedPage(int page)
        {
            currentPage = page;
            await LoadAsync(page);
        }

        private async Task LoadListAsync(int page)
        {
            var url = $"api/UserCredits";
            /*if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }*/
            var response = await Repository.GetAsync<List<UserCreditDTO>>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            UserCredits = response.Response;
            return;
        }

        private async Task LoadPagesAsync()
        {
            var url = "api/accounts/totalPages";
            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"?filter={Filter}";
            }

            var response = await Repository.GetAsync<int>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            totalPages = response.Response;
        }

        private async Task ApplyFilterAsync()
        {
            await LoadAsync();
        }

        private async Task RefreshCreditsList()
        {
            await LoadAsync();
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            await LoadAsync();
        }

        private async Task OpenModal(UserCreditDTO? user = null)
        {
            addCreditModal.SelectedUser = user; // si es null -> nuevo crédito
            await JS.InvokeVoidAsync("bootstrapModal.show", "addCreditModal");
        }
    }
}