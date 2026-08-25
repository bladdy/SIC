using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Shared.Component.Modals
{
    public partial class AddCreditModal
    {
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
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
                var responseHttp = await Repository.GetAsync<List<User>>("api/Accounts/all?PageSize=200");
                if (responseHttp.Error)
                {
                    var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudieron cargar los planners.";
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return;
                }

                Planners = responseHttp.Response?
                    .Where(u => u.UserType == UserType.WeddingPlanner)
                    .ToList() ?? new List<User>();
            }
            catch
            {
                await SweetAlertService.FireAsync("Error", "No se pudieron cargar los planners.", SweetAlertIcon.Error);
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
            if (string.IsNullOrEmpty(model.UserId))
                return;

            isSaving = true;

            var responseHttp = await Repository.PostAsync<AddCreditsRequest>("api/UserCredits/add", model);

            isSaving = false;

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudieron agregar los créditos.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await JS.InvokeVoidAsync("bootstrapModal.hide", "addCreditModal");

            if (OnCreditsAdded.HasDelegate)
                await OnCreditsAdded.InvokeAsync();

            // Limpiar si no es un usuario preseleccionado
            if (!isPreselectedUser)
            {
                ResetForm();
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