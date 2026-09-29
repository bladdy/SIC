using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.Plans
{
    [Authorize(Roles = "Admin")]
    public partial class PlansIndex
    {
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

        public List<Plan>? Plans { get; set; }
        private Plan NewPlan = new();
        private bool IsModalVisible = false;
        private bool IsEditMode = false;  // Nuevo flag

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadPlans();
        }

        private async Task LoadPlans()
        {
            var responseHttp = await repository.GetAsync<List<Plan>>("api/Plans");
            Plans = responseHttp.Response;
        }

        private void ShowCreateModal()
        {
            NewPlan = new Plan();
            IsEditMode = false;
            IsModalVisible = true;
        }

        private void ShowEditModal(Plan plan)
        {
            // Clonar el objeto para no afectar la lista si cancelamos
            NewPlan = new Plan
            {
                Id = plan.Id,
                Name = plan.Name,
                Price = plan.Price,
            };
            IsEditMode = true;
            IsModalVisible = true;
        }

        private void CloseModal()
        {
            IsModalVisible = false;
        }

        private async Task SavePlan()
        {
            HttpResponseWrapper<object>? responseHttp;

            if (IsEditMode)
            {
                // PUT -> Editar
                responseHttp = await repository.PutAsync("api/Plans", NewPlan);
            }
            else
            {
                // POST -> Crear
                responseHttp = await repository.PostAsync<Plan>("api/Plans", NewPlan);
            }

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el plan.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            // Cerrar el modal inmediatamente al confirmar que la operaci�n fue exitosa
            CloseModal();

            // Luego mostrar la notificaci�n
            await sweetAlertService.FireAsync(
                "�xito",
                IsEditMode ? "Plan actualizado con �xito." : "Plan creado con �xito.",
                SweetAlertIcon.Success
            );

            await LoadPlans();
        }

private async Task ConfirmDelete(Plan plan)
        {
            _pendingPlan = plan;
            ConfirmMessage = $"Se eliminará el plan '{plan.Name}'. Esta acción no se puede deshacer.";
            IsConfirmVisible = true;
            await Task.CompletedTask;
        }

        private bool IsConfirmVisible;
        private string ConfirmMessage = "";
        private Plan? _pendingPlan;

        private async Task OnConfirmVisibleChanged(bool visible)
        {
            IsConfirmVisible = visible;
            if (!visible) _pendingPlan = null;
        }

        private async Task ExecutePendingDelete()
        {
            if (_pendingPlan == null) return;
            await DeletePlan(_pendingPlan);
_pendingPlan = null;
        }

        private async Task DeletePlan(Plan plan)
        {
            var responseHttp = await repository.DeleteAsync<Plan>($"api/Plans/{plan.Id}");

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el plan.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await sweetAlertService.FireAsync("Eliminado", "El plan fue borrado correctamente.", SweetAlertIcon.Success);

            await LoadPlans();
        }
    }
}