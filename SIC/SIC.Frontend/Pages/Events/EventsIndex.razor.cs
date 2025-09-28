using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

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

        private async Task SaveEvent()
        {
            HttpResponseWrapper<object>? responseHttp;
            if (IsEditMode)
            {
                responseHttp = await repository.PutAsync<Event>($"api/Events/{NewEvent.Id}", NewEvent);
            }
            else
            {
                responseHttp = await repository.PostAsync<Event>("api/Events", NewEvent);
            }
            if (!responseHttp.Error)
            {
                await sweetAlertService.FireAsync("Éxito", "El evento se ha guardado correctamente.", SweetAlertIcon.Success);
                IsModalVisible = false;
                await LoadEvents();
            }
            else
            {
                var errorMessage = await responseHttp.GetErrorMessageAsync();
                await sweetAlertService.FireAsync("Error", errorMessage ?? "Ha ocurrido un error inesperado.", SweetAlertIcon.Error);
            }
        }

        private async Task ConfirmDelete(Event events)
        {
            var result = await sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "¿Está seguro?",
                Text = $"Se eliminará el evento '{events.Name}'. Esta acción no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, borrar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                await DeletePlan(events);
            }
        }

        private async Task DeletePlan(Event events)
        {
            var responseHttp = await repository.DeleteAsync<Event>($"api/Event/{events.Id}");

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
    }
}