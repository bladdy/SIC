using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;
using System.Security.Claims;

namespace SIC.Frontend.Pages.Events
{
    [Authorize(Roles = "Admin")]
    public partial class EventsIndex
    {
        private string? _userId;
        private int currentPage = 1;
        private int totalPages;

        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public DateTime? DateSelectd { get; set; } = null;
        [Parameter, SupplyParameterFromQuery] public int? SelectedEventType { get; set; }
        [Parameter, SupplyParameterFromQuery] public string OrderBy { get; set; } = "";
        [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }

        public List<Event>? Events { get; set; }
        public List<EventType>? EventTypes { get; set; }

        private Event NewEvent = new();
        private bool IsModalVisible = false;
        private bool IsEditMode = false;
        private DateTime MinAllowedDate { get; set; } = new DateTime(2023, 1, 1);

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // Si RecordsNumber viene null, lo asignamos a 15
            RecordsNumber ??= 15;

            await LoadEventTypes();
            await LoadEvents(currentPage);
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadEvents(currentPage);
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            SelectedEventType = null;
            DateSelectd = null;
            OrderBy = "";
            await ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadEvents(page);
        }

        private async Task LoadEvents(int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(Page))
            {
                page = Convert.ToInt32(Page);
            }
            var ok = await LoadListAsync(page);
            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private async Task<bool> LoadListAsync(int page)
        {
            var url = $"api/Events/paginated?PageNumber={page}&PageSize={RecordsNumber}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&Filter={Filter}";
            }
            if (DateSelectd != null)
            {
                url += $"&Date={DateSelectd}";
            }
            if (SelectedEventType != null)
            {
                url += $"&EventTypeId={SelectedEventType}";
            }
            if (!string.IsNullOrWhiteSpace(OrderBy))
            {
                url += $"&OrderBy={OrderBy}";
            }
            var responseHttp = await repository.GetAsync<List<Event>>(url);

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/events");
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return false;
                }
            }

            Events = responseHttp?.Response ?? new List<Event>();
            return true;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/Events/totalRecords?RecordsNumber={RecordsNumber}";

            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&Filter={Filter}";
            }
            if (DateSelectd != null)
            {
                url += $"&Date={DateSelectd}";
            }
            if (SelectedEventType != null)
            {
                url += $"&EventTypeId={SelectedEventType}";
            }

            var responseHttp = await repository.GetAsync<int>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            totalPages = responseHttp.Response;
        }

        private async Task LoadEventTypes()
        {
            var responseHttp = await repository.GetAsync<List<EventType>>("api/EventTypes");
            if (!responseHttp.Error && responseHttp.Response != null)
            {
                EventTypes = responseHttp.Response;
            }
        }

        private async Task ShowCreateModal()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                NewEvent = new Event
                {
                    UserId = userId ?? string.Empty
                };
            }
            else
            {
                NewEvent = new Event();
            }

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
                UserId = evnt.UserId,
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
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
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
                await DeleteEvents(events);
            }
        }

        private async Task DeleteEvents(Event events)
        {
            var responseHttp = await repository.DeleteAsync<Event>($"api/Events/{events.Id}");

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el Evento.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync("Eliminado", "El Evento fue borrado correctamente.", SweetAlertIcon.Success);

            await LoadEvents(currentPage);
        }

        private async Task SaveEvent()
        {
            HttpResponseWrapper<object>? responseHttp;

            if (IsEditMode)
            {
                responseHttp = await repository.PutAsync("api/events/full", NewEvent);
            }
            else
            {
                responseHttp = await repository.PostAsync("api/events/full", NewEvent);
            }

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el evento.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            CloseModal();

            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync(
                "Éxito",
                IsEditMode ? "Evento actualizado con éxito." : "Evento creado con éxito.",
                SweetAlertIcon.Success
            );

            await LoadEvents(currentPage);
        }
    }
}