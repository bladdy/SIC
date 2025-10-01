using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.Services
{
    [Authorize(Roles = "Admin")]
    public partial class ServicesIndex
    {
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

        public List<Item>? Items { get; set; }
        private Item NewItem = new();
        private bool IsModalVisible = false;
        private bool IsEditMode = false;  // Nuevo flag

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadItem();
        }

        private async Task LoadItem()
        {
            var responseHttp = await repository.GetAsync<List<Item>>("api/Items");
            Items = responseHttp.Response;
        }

        private void ShowCreateModal()
        {
            NewItem = new Item();
            IsEditMode = false;
            IsModalVisible = true;
        }

        private void ShowEditModal(Item item)
        {
            // Clonar el objeto para no afectar la lista si cancelamos
            NewItem = new Item
            {
                Id = item.Id,
                Name = item.Name,
            };
            IsEditMode = true;
            IsModalVisible = true;
        }

        private void CloseModal()
        {
            IsModalVisible = false;
        }

        private async Task SaveItem()
        {
            HttpResponseWrapper<object>? responseHttp;

            if (IsEditMode)
            {
                // PUT -> Editar
                responseHttp = await repository.PutAsync("api/Items", NewItem);
            }
            else
            {
                // POST -> Crear
                responseHttp = await repository.PostAsync<Item>("api/Items", NewItem);
            }

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el Servicio.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            // Cerrar el modal inmediatamente al confirmar que la operaci�n fue exitosa
            CloseModal();

            // Luego mostrar la notificaci�n
            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync(
                "�xito",
                IsEditMode ? "Servicio actualizado con �xito." : "Servicio creado con �xito.",
                SweetAlertIcon.Success
            );

            await LoadItem();
        }

        private async Task ConfirmDelete(Item item)
        {
            var result = await sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "�Est� seguro?",
                Text = $"Se eliminar� el Servicio '{item.Name}'. Esta acci�n no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "S�, borrar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                await DeleteItem(item);
            }
        }

        private async Task DeleteItem(Item item)
        {
            var responseHttp = await repository.DeleteAsync<Item>($"api/Items/{item.Id}");

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el Servicio.";
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
            await toast.FireAsync("Eliminado", "El Servicio fue borrado correctamente.", SweetAlertIcon.Success);

            await LoadItem();
        }
    }
}