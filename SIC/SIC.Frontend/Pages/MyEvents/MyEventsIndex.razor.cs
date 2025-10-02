using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Repositories;
using SIC.Frontend.Shared;
using SIC.Shared.Entities;
using System.Net;
using System.Security.Claims;

namespace SIC.Frontend.Pages.MyEvents;

[Authorize(Roles = "Admin,WeddingPlanner,User")]
public partial class MyEventsIndex
{
    private string? _userId;
    private int currentPage = 1;
    private int totalPages;
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;
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
    private DateTime MinAllowedDate { get; set; } = new DateTime(2023, 1, 1); // Sets January 1, 2023 as the minimum

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _userId = userId;
            NewEvent.UserId = userId ?? string.Empty;
        }
        RecordsNumber ??= 15;

        await LoadEventTypes();
        await LoadEvents(currentPage);
    }

    private async Task LoadEvents()
    {
        var responseHttp = await repository.GetAsync<List<Event>>($"api/Events/byUserId/{NewEvent.UserId}") ?? null;
        Events = responseHttp?.Response;
    }

    private async Task<bool> LoadListAsync(int page)
    {
        var url = $"api/Events/paginated?UserId={_userId}&PageNumber={page}&PageSize={RecordsNumber}";

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
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
        }

        Events = responseHttp?.Response ?? new List<Event>();
        return true;
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
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        totalPages = responseHttp.Response;
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

    private async Task SelectedPageAsync(int page)
    {
        currentPage = page;
        await LoadEvents(currentPage);
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