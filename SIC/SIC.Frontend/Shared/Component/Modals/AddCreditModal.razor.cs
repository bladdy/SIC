using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.Net.Http.Json;

namespace SIC.Frontend.Shared.Component.Modals
{
    public partial class AddCreditModal
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Parameter] public EventCallback OnCreditsAdded { get; set; }
        [Parameter] public UserCreditDTO? SelectedUser { get; set; }

        private List<User> Planners = new();
        private string filterText = string.Empty;
        private bool isLoading = true;
        private bool isSaving = false;
        private bool isPreselectedUser = false;

        private AddCreditsRequest model = new()
        {
            UpdatedBy = "Administrador"
        };

        protected override async Task OnInitializedAsync()
        {
            await LoadPlannersAsync();
        }

        protected override void OnParametersSet()
        {
            if (SelectedUser != null)
            {
                model.UserId = SelectedUser.UserId;
                filterText = SelectedUser.FullName;
                isPreselectedUser = true;
            }
            else
            {
                isPreselectedUser = false;
            }
        }

        private async Task LoadPlannersAsync()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<List<User>>("api/Accounts/all");
                if (result != null)
                    Planners = result.Where(u => u.UserType == UserType.WeddingPlanner).ToList();
            }
            catch
            {
                // Manejo de error
            }
            finally
            {
                isLoading = false;
            }
        }

        private IEnumerable<User> FilteredUsers =>
            string.IsNullOrWhiteSpace(filterText)
                ? Planners
                : Planners.Where(u => u.FullName.Contains(filterText, StringComparison.OrdinalIgnoreCase));

        private async Task AddCreditsAsync()
        {
            if (string.IsNullOrEmpty(model.UserId) || model.CreditsToAdd <= 0)
                return;

            isSaving = true;

            var response = await Http.PostAsJsonAsync("api/UserCredits/add", model);

            isSaving = false;

            if (response.IsSuccessStatusCode)
            {
                await JS.InvokeVoidAsync("bootstrapModal.hide", "addCreditModal");

                if (OnCreditsAdded.HasDelegate)
                    await OnCreditsAdded.InvokeAsync();

                // Limpiar si no es un usuario preseleccionado
                if (!isPreselectedUser)
                {
                    ResetForm();
                }
            }
        }

        public async Task Close()
        {
            await JS.InvokeVoidAsync("bootstrapModal.hide", "addCreditModal");
            ResetForm();
        }

        private void ResetForm()
        {
            model = new AddCreditsRequest
            {
                UpdatedBy = "Administrador"
            };
            filterText = string.Empty;
            SelectedUser = null;
            isPreselectedUser = false;
        }
    }
}