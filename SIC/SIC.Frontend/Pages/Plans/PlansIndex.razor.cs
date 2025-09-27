using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.Plans
{
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

            // Cerrar el modal inmediatamente al confirmar que la operación fue exitosa
            CloseModal();

            // Luego mostrar la notificación
            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync(
                "Éxito",
                IsEditMode ? "Plan actualizado con éxito." : "Plan creado con éxito.",
                SweetAlertIcon.Success
            );

            await LoadPlans();
        }

        private async Task ConfirmDelete(Plan plan)
        {
            var result = await sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "¿Está seguro?",
                Text = $"Se eliminará el plan '{plan.Name}'. Esta acción no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, borrar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                await DeletePlan(plan);
            }
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

            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync("Eliminado", "El plan fue borrado correctamente.", SweetAlertIcon.Success);

            await LoadPlans();
        }
    }
}