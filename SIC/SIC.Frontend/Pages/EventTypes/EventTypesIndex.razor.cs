using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.EventTypes;

[Authorize(Roles = "Admin")]
public partial class EventTypesIndex
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

    public List<EventType>? EventsType { get; set; }
    private EventType NewEventType = new();
    private bool IsModalVisible = false;
    private bool IsEditMode = false;  // Nuevo flag

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadEventTypes();
    }

    private async Task LoadEventTypes()
    {
        var responseHttp = await repository.GetAsync<List<EventType>>("api/EventTypes");
        EventsType = responseHttp.Response;
    }

    private void ShowCreateModal()
    {
        NewEventType = new EventType();
        IsEditMode = false;
        IsModalVisible = true;
    }

    private void ShowEditModal(EventType eventType)
    {
        // Clonar el objeto para no afectar la lista si cancelamos
        NewEventType = new EventType
        {
            Id = eventType.Id,
            Name = eventType.Name,
        };
        IsEditMode = true;
        IsModalVisible = true;
    }

    private void CloseModal()
    {
        IsModalVisible = false;
    }

    private async Task SaveEventTypes()
    {
        HttpResponseWrapper<object>? responseHttp;

        if (IsEditMode)
        {
            // PUT -> Editar
            responseHttp = await repository.PutAsync("api/EventTypes", NewEventType);
        }
        else
        {
            // POST -> Crear
            responseHttp = await repository.PostAsync<EventType>("api/EventTypes", NewEventType);
        }

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el Tipo de evento.";
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
            IsEditMode ? "Tipo de evento actualizado con �xito." : "Tipo de evento creado con �xito.",
            SweetAlertIcon.Success
        );

        await LoadEventTypes();
    }

    private async Task ConfirmDelete(EventType eventType)
    {
        var result = await sweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = "�Est� seguro?",
            Text = $"Se eliminar� el Tipo de evento '{eventType.Name}'. Esta acci�n no se puede deshacer.",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "S�, borrar",
            CancelButtonText = "Cancelar"
        });

        if (!string.IsNullOrEmpty(result.Value))
        {
            await DeleteEventTypes(eventType);
        }
    }

    private async Task DeleteEventTypes(EventType eventType)
    {
        var responseHttp = await repository.DeleteAsync<EventType>($"api/EventTypes/{eventType.Id}");

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el Tipo de evento.";
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
        await toast.FireAsync("Eliminado", "El Tipo de evento fue borrado correctamente.", SweetAlertIcon.Success);

        await LoadEventTypes();
    }
}