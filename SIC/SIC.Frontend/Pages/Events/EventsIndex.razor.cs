using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.Events
{
    [Authorize(Roles = "Admin")]
    public partial class EventsIndex
    {
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        private List<Event>? Events;
        private List<EventType>? EventTypes;
        private List<User> AllUsers = new();

        private Event NewEvent = new();
        private bool IsModalVisible = false;
        private bool IsEditMode = false;

        private int currentPage = 1;
        private int totalPages;

        private string filterText = string.Empty;
        private bool isPreselectedUser = false;

        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public DateTime? DateSelectd { get; set; } = null;
        [Parameter, SupplyParameterFromQuery] public int? SelectedEventType { get; set; }
        [Parameter, SupplyParameterFromQuery] public string OrderBy { get; set; } = "";
        [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }

        protected override async Task OnInitializedAsync()
        {
            RecordsNumber ??= 15;
            await LoadEventTypes();
            await LoadUsersAsync();
            await LoadEvents(currentPage);
        }

        //  Cargar todos los usuarios disponibles
        private async Task LoadUsersAsync()
        {
            var result = await repository.GetAsync<List<User>>("api/Accounts/all?PageSize=100");
            if (result != null)
            {
                // Excluir Admin y WeddingPlanner
                AllUsers = result.Response!
                    //.Where(u => u.UserType != UserType.Admin && u.UserType != UserType.WeddingPlanner)
                    .ToList();
            }
        }

        //  Filtrado de usuarios
        private IEnumerable<User> FilteredUsers =>
            string.IsNullOrWhiteSpace(filterText)
                ? AllUsers
                : AllUsers.Where(u =>
                    u.FullName.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                    u.PhoneNumber!.Contains(filterText, StringComparison.OrdinalIgnoreCase) ||
                    u.Email!.Contains(filterText, StringComparison.OrdinalIgnoreCase));

        private async Task LoadEventTypes()
        {
            var response = await repository.GetAsync<List<EventType>>("api/EventTypes");
            if (!response.Error)
                EventTypes = response.Response;
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

        private async Task LoadPagesAsync()
        {
            var url = $"api/Events/totalRecords?PageSize={RecordsNumber}";

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

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadEvents(page);
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadEvents(page);
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            await LoadEvents(1);
        }

        //  Crear nuevo evento
        private async Task ShowCreateModal()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            NewEvent = new Event();
            isPreselectedUser = false; // Se puede seleccionar cualquier usuario
            IsEditMode = false;
            IsModalVisible = true;
        }

        //  Editar evento existente
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
                CoverImageUrl = evnt.CoverImageUrl,
                CoverAlbumImageUrl = evnt.CoverAlbumImageUrl,
                Ubication = evnt.Ubication,
                Host = evnt.Host,
                HostPhone = evnt.HostPhone,
                Planner = evnt.Planner,
                AlbumPublic = evnt.AlbumPublic,
                HasAlbum = evnt.HasAlbum,
                OnlyAlbum = evnt.OnlyAlbum,
                PlannerPhone = evnt.PlannerPhone,
                EventType = evnt.EventType,
                Status = evnt.Status
            };

            // Ahora el usuario también puede cambiar la asignación
            isPreselectedUser = false;
            IsEditMode = true;
            IsModalVisible = true;
        }

        private void CloseModal() => IsModalVisible = false;

        // Confirmar eliminación
        private async Task ConfirmDelete(Event evnt)
        {
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "¿Eliminar evento?",
                Text = $"Se eliminará '{evnt.Name}'. Esta acción no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true
            });

            if (!string.IsNullOrEmpty(result.Value))
                await DeleteEvent(evnt);
        }

        private async Task DeleteEvent(Event evnt)
        {
            var response = await repository.DeleteAsync<Event>($"api/Events/{evnt.Id}");
            if (!response.Error)
            {
                await SweetAlertService.FireAsync("Eliminado", "Evento borrado correctamente.", SweetAlertIcon.Success);
                await LoadEvents(currentPage);
            }
        }

        // Guardar evento (crear o editar)
        private async Task SaveEvent()
        {
            if (string.IsNullOrEmpty(NewEvent.UserId))
            {
                await SweetAlertService.FireAsync("Error", "Debes asignar un usuario al evento.", SweetAlertIcon.Error);
                return;
            }
            var HostUser = AllUsers.FirstOrDefault(u => u.Id == NewEvent.UserId);
            if (HostUser == null)
            {
                await SweetAlertService.FireAsync("Error", "El usuario asignado no es válido.", SweetAlertIcon.Error);
                return;
            }
            if (HostUser.UserCredit != null)
            {
                if ((HostUser.UserType.ToString() == "WeddingPlanner") && (HostUser.UserCredit?.AvailableCredits <= 0))
                {
                    await SweetAlertService.FireAsync("Error", "El usuario asignado no tiene creditos dis ponibles.", SweetAlertIcon.Error);
                    return;
                }
            }
            NewEvent.Host = HostUser.FullName;
            NewEvent.HostPhone = HostUser.PhoneNumber!;
            HttpResponseWrapper<object>? response;
            if (IsEditMode)
                response = await repository.PutAsync("api/Events/full", NewEvent);
            else
                response = await repository.PostAsync("api/Events/full", NewEvent);

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "No se pudo guardar el evento.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            if ((HostUser.UserType.ToString() == "WeddingPlanner") && !response.Error && IsEditMode == false)
            {
                HttpResponseWrapper<object>? responseHttps;
                responseHttps = await repository.PostAsync<object>($"api/UserCredits/consume/{NewEvent.UserId}/{NewEvent.Name}");
            }
            await SweetAlertService.FireAsync("Éxito", IsEditMode ? "Evento actualizado." : "Evento creado.", SweetAlertIcon.Success);
            CloseModal();
            await LoadEvents(currentPage);
            await LoadUsersAsync();
        }
    }
}