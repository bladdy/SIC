using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using SIC.Shared.Helpers;

namespace SIC.Frontend.Pages.Events
{
    public partial class EventsIndex
    {
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;
        public List<Event>? Events { get; set; }
        public List<EventType>? EventTypes { get; set; }
        private Event NewEvent = new();
        private bool IsModalVisible = false;
        private bool IsEditMode = false;
        private DateTime MinAllowedDate { get; set; } = new DateTime(2023, 1, 1); // Sets January 1, 2023 as the minimum

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadEventTypes();
            await LoadEvents();
        }

        private async Task LoadEvents()
        {
            var responseHttp = await repository.GetAsync<List<Event>>("api/Events") ?? null;
            Events = responseHttp?.Response;
        }

        private async Task LoadEventTypes()
        {
            var responseHttp = await repository.GetAsync<List<EventType>>("api/EventTypes");
            if (!responseHttp.Error && responseHttp.Response != null)
            {
                EventTypes = responseHttp.Response;
            }
        }

        private void ShowCreateModal()
        {
            NewEvent = new Event();
            IsEditMode = false;
            IsModalVisible = true;
        }

        private void ShowEditModal(Event evnt)
        {
            NewEvent = new Event
            {
                Id = evnt.Id,
                Code = evnt.Code,
                Name = evnt.Name,
                SubTitle = evnt.SubTitle,
                EventTypeId = evnt.EventTypeId,
                Date = evnt.Date,
                Time = evnt.Time,
                Url = evnt.Url,
                Host = evnt.Host,
                HostPhone = evnt.HostPhone,
                Planner = evnt.Planner,
                PlannerPhone = evnt.PlannerPhone,
                EventType = evnt.EventType,
                Status = evnt.Status
            };
            IsEditMode = true;
            IsModalVisible = true;
        }

        private void CloseModal()
        {
            IsModalVisible = false;
        }

        private async Task ConfirmDelete(Event events)
        {
            var result = await sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "�Est� seguro?",
                Text = $"Se eliminar� el evento '{events.Name}'. Esta acci�n no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "S�, borrar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                await DeleteEvents(events);
            }
        }

        private async Task DeleteEvents(Event events)
        {
            var responseHttp = await repository.DeleteAsync<Event>($"api/Events/{events.Id}");

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el Evento.";
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
            await toast.FireAsync("Eliminado", "El Evento fue borrado correctamente.", SweetAlertIcon.Success);

            await LoadEvents();
        }

        private async Task SaveEvent()
        {
            HttpResponseWrapper<object>? responseHttp;

            if (IsEditMode)
            {
                // PUT -> Editar
                responseHttp = await repository.PutAsync("api/events/full", NewEvent);
            }
            else
            {
                // POST -> Crear
                responseHttp = await repository.PostAsync("api/events/full", NewEvent);
            }

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el plan.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

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
                IsEditMode ? "Plan actualizado con �xito." : "Plan creado con �xito.",
                SweetAlertIcon.Success
            );

            await LoadEvents();
        }
    }
}