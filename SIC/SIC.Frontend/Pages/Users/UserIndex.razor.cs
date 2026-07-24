using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using static System.Net.WebRequestMethods;

namespace SIC.Frontend.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public partial class UserIndex
    {
        public List<User>? Users { get; set; }
        private int currentPage = 1;
        private int totalPages;

        private bool ShowModal = false;

        private bool ShowCreate = false;
        private bool ShowEdit = false;
        private User? SelectedUser;

        private void ShowCreateModal(bool show) => ShowCreate = show;

        private void ShowEditModal(User user)
        {
            SelectedUser = user;
            ShowEdit = true;
        }

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }

        protected override async Task OnInitializedAsync()
        {
            RecordsNumber ??= 15;
            if (!string.IsNullOrWhiteSpace(Page) && int.TryParse(Page, out var pageFromQuery))
            {
                currentPage = pageFromQuery;
            }
            await LoadAsync(currentPage);
        }

        private async Task LoadAsync(int page = 1)
        {
            var ok = await LoadListAsync(page);
            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private async Task LoadUsers()
        {
            await LoadAsync();
        }

        private async Task SelectedPage(int page)
        {
            currentPage = page;
            await LoadAsync(page);
        }

        private async Task<bool> LoadListAsync(int page)
        {
            var url = $"api/accounts/all?PageNumber={page}&PageSize={RecordsNumber ?? 15}";
            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }
            var response = await Repository.GetAsync<List<User>>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            Users = response.Response;
            return true;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/accounts/totalPages?PageSize={RecordsNumber ?? 15}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&filter={Uri.EscapeDataString(Filter)}";
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

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            await LoadAsync();
        }
    }
}