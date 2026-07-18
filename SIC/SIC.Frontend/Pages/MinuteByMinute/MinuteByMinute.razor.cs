using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Pages.MinuteByMinute;

public partial class MinuteByMinute
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

    [Parameter] public string? EventId { get; set; }

    private string EventName = "";
    private string ActiveTab = "timeline";

    private MinuteByMinuteContainer? MbMContainer;
    private List<MbMActivity> Activities = new();
    private List<MbMActivity> FilteredActivities => Activities
        .Where(a =>
            (string.IsNullOrWhiteSpace(FilterText) || a.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase) || (a.Description != null && a.Description.Contains(FilterText, StringComparison.OrdinalIgnoreCase)))
            && (FilterStatus == "" || a.Status.ToString() == FilterStatus)
            && (FilterPriority == "" || a.Priority.ToString() == FilterPriority))
        .OrderBy(a => a.StartTime)
        .ToList();

    private string FilterText = "";
    private string FilterStatus = "";
    private string FilterPriority = "";

    private int TotalActivities => Activities.Count;
    private int CompletedCount => Activities.Count(a => a.Status == ActivityStatus.Completada);
    private int InProgressCount => Activities.Count(a => a.Status == ActivityStatus.EnProgreso);
    private int PendingCount => Activities.Count(a => a.Status == ActivityStatus.Pendiente || a.Status == ActivityStatus.Cancelada);

    private bool IsActivityModalVisible;
    private bool IsActivityEditMode;
    private MbMActivity EditingActivity = new();

    private bool IsProviderModalVisible;
    private MbMProvider EditingProvider = new();
    private MbMActivity? ProviderTargetActivity;

    private bool IsTaskModalVisible;
    private MbMTask EditingTask = new();
    private MbMActivity? TaskTargetActivity;

    private bool IsConfirmVisible;
    private MbMActivity? ConfirmTargetActivity;
    private string ConfirmMessage = "";
    private string ConfirmType = "";
    private object? ConfirmTarget;

    private bool IsLoading = true;
    private bool ShowCreateMbM;
    private string NewMbMTitle = "";
    private int NumericEventId;

    protected override async Task OnInitializedAsync()
    {
        await LoadMbMAsync();
    }

    private async Task LoadMbMAsync()
    {
        IsLoading = true;
        ShowCreateMbM = false;
        StateHasChanged();

        if (!string.IsNullOrWhiteSpace(EventId))
        {
            var response = await repository.GetAsync<MinuteByMinuteContainer>($"api/MinuteByMinute/byEventCode/{EventId}");
            if (!response.Error && response.Response != null)
            {
                MbMContainer = response.Response;
                NumericEventId = MbMContainer.EventId;
                EventName = MbMContainer.Event?.Name ?? "";
                Activities = MbMContainer.Activities ?? new List<MbMActivity>();
            }
            else
            {
                MbMContainer = null;
                Activities = new List<MbMActivity>();

                var eventResponse = await repository.GetAsync<Event>($"api/Events/byCode/{EventId}");
                if (!eventResponse.Error && eventResponse.Response != null)
                {
                    NumericEventId = eventResponse.Response.Id;
                    EventName = eventResponse.Response.Name;
                }

                ShowCreateMbM = true;
            }
        }

        IsLoading = false;
        StateHasChanged();
    }

    private async Task CreateMbMAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMbMTitle))
        {
            await sweetAlertService.FireAsync("Error", "El título es obligatorio.", SweetAlertIcon.Error);
            return;
        }

        var dto = new MinuteByMinuteDTO { Title = NewMbMTitle.Trim() };
        var response = await repository.PostAsync<MinuteByMinuteDTO, MinuteByMinuteContainer>($"api/MinuteByMinute/byEventId/{NumericEventId}", dto);
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "Error al crear el Minuto a Minuto.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        NewMbMTitle = "";
        await sweetAlertService.FireAsync("Éxito", "Minuto a Minuto creado correctamente.", SweetAlertIcon.Success);
        await LoadMbMAsync();
    }

    private void OpenCreateActivityModal()
    {
        EditingActivity = new MbMActivity { StartTime = DateTime.Now };
        IsActivityEditMode = false;
        IsActivityModalVisible = true;
    }

    private void OpenEditActivityModal(MbMActivity activity)
    {
        EditingActivity = new MbMActivity
        {
            Id = activity.Id,
            Title = activity.Title,
            Description = activity.Description,
            StartTime = activity.StartTime,
            EndTime = activity.EndTime,
            Status = activity.Status,
            Priority = activity.Priority,
            Location = activity.Location,
            Notes = activity.Notes
        };
        IsActivityEditMode = true;
        IsActivityModalVisible = true;
    }

    private void CloseActivityModal() => IsActivityModalVisible = false;

    private async Task SaveActivity()
    {
        if (MbMContainer == null) return;

        if (IsActivityEditMode)
        {
            var dto = new MbMActivityDTO
            {
                Title = EditingActivity.Title,
                Description = EditingActivity.Description,
                StartTime = EditingActivity.StartTime,
                EndTime = EditingActivity.EndTime,
                Status = EditingActivity.Status,
                Priority = EditingActivity.Priority,
                Location = EditingActivity.Location,
                Notes = EditingActivity.Notes
            };
            var response = await repository.PutAsync($"api/MbMActivities/{EditingActivity.Id}", dto);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "Error al actualizar la actividad.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }
        else
        {
            var dto = new MbMActivityDTO
            {
                Title = EditingActivity.Title,
                Description = EditingActivity.Description,
                StartTime = EditingActivity.StartTime,
                EndTime = EditingActivity.EndTime,
                Status = EditingActivity.Status,
                Priority = EditingActivity.Priority,
                Location = EditingActivity.Location,
                Notes = EditingActivity.Notes
            };
            var response = await repository.PostAsync<MbMActivityDTO, MbMActivity>($"api/MbMActivities/ByMinuteByMinuteId/{MbMContainer!.Id}", dto);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "Error al crear la actividad.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }

        IsActivityModalVisible = false;
        await LoadMbMAsync();
    }

    private void ConfirmDeleteActivity(MbMActivity activity)
    {
        ConfirmTargetActivity = activity;
        ConfirmMessage = $"¿Estás seguro de eliminar la actividad \"{activity.Title}\"?";
        ConfirmType = "activity";
        IsConfirmVisible = true;
    }

    private async Task DeleteActivity()
    {
        if (ConfirmTargetActivity == null) return;

        var response = await repository.DeleteAsync<MbMActivity>($"api/MbMActivities/{ConfirmTargetActivity.Id}");
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "Error al eliminar la actividad.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
        }

        ConfirmTargetActivity = null;
        IsConfirmVisible = false;
        await LoadMbMAsync();
    }

    private void CloseConfirmModal()
    {
        IsConfirmVisible = false;
        ConfirmTargetActivity = null;
        ConfirmTarget = null;
    }

    private async Task ConfirmAction()
    {
        if (ConfirmType == "activity")
            await DeleteActivity();
        else if (ConfirmType == "provider")
            await DeleteProvider();
        else if (ConfirmType == "task")
            await DeleteTask();
    }

    private void OpenProviderModal(MbMActivity activity, MbMProvider? provider = null)
    {
        ProviderTargetActivity = activity;
        EditingProvider = provider != null
            ? new MbMProvider { Id = provider.Id, Name = provider.Name, Contact = provider.Contact, Service = provider.Service, Status = provider.Status, Cost = provider.Cost }
            : new MbMProvider();
        IsProviderModalVisible = true;
    }

    private void CloseProviderModal() => IsProviderModalVisible = false;

    private async Task SaveProvider()
    {
        if (ProviderTargetActivity == null) return;

        if (EditingProvider.Id > 0)
        {
            var dto = new MbMProviderDTO
            {
                Name = EditingProvider.Name,
                Contact = EditingProvider.Contact,
                Service = EditingProvider.Service,
                Status = EditingProvider.Status,
                Cost = EditingProvider.Cost
            };
            var response = await repository.PutAsync($"api/MbMProviders/{EditingProvider.Id}", dto);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "Error al actualizar el proveedor.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }
        else
        {
            var dto = new MbMProviderDTO
            {
                Name = EditingProvider.Name,
                Contact = EditingProvider.Contact,
                Service = EditingProvider.Service,
                Status = EditingProvider.Status,
                Cost = EditingProvider.Cost
            };
            var response = await repository.PostAsync<MbMProviderDTO, MbMProvider>($"api/MbMProviders/ByActivityId/{ProviderTargetActivity.Id}", dto);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "Error al crear el proveedor.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }

        IsProviderModalVisible = false;
        await LoadMbMAsync();
    }

    private void ConfirmDeleteProvider(MbMActivity activity, MbMProvider provider)
    {
        ProviderTargetActivity = activity;
        EditingProvider = provider;
        ConfirmMessage = $"¿Estás seguro de eliminar el proveedor \"{provider.Name}\"?";
        ConfirmType = "provider";
        IsConfirmVisible = true;
    }

    private async Task DeleteProvider()
    {
        if (EditingProvider.Id <= 0) return;

        var response = await repository.DeleteAsync<MbMProvider>($"api/MbMProviders/{EditingProvider.Id}");
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "Error al eliminar el proveedor.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
        }

        IsConfirmVisible = false;
        await LoadMbMAsync();
    }

    private void OpenTaskModal(MbMActivity activity, MbMTask? task = null)
    {
        TaskTargetActivity = activity;
        EditingTask = task != null
            ? new MbMTask { Id = task.Id, Title = task.Title, IsCompleted = task.IsCompleted, AssignedTo = task.AssignedTo, DueDate = task.DueDate, Priority = task.Priority }
            : new MbMTask();
        IsTaskModalVisible = true;
    }

    private void CloseTaskModal() => IsTaskModalVisible = false;

    private async Task SaveTask()
    {
        if (TaskTargetActivity == null) return;

        if (EditingTask.Id > 0)
        {
            var dto = new MbMTaskDTO
            {
                Title = EditingTask.Title,
                IsCompleted = EditingTask.IsCompleted,
                AssignedTo = EditingTask.AssignedTo,
                DueDate = EditingTask.DueDate,
                Priority = EditingTask.Priority
            };
            var response = await repository.PutAsync($"api/MbMTasks/{EditingTask.Id}", dto);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "Error al actualizar la tarea.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }
        else
        {
            var dto = new MbMTaskDTO
            {
                Title = EditingTask.Title,
                IsCompleted = EditingTask.IsCompleted,
                AssignedTo = EditingTask.AssignedTo,
                DueDate = EditingTask.DueDate,
                Priority = EditingTask.Priority
            };
            var response = await repository.PostAsync<MbMTaskDTO, MbMTask>($"api/MbMTasks/ByActivityId/{TaskTargetActivity.Id}", dto);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "Error al crear la tarea.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }

        IsTaskModalVisible = false;
        await LoadMbMAsync();
    }

    private async Task ToggleTask(MbMActivity activity, MbMTask task)
    {
        var response = await repository.PutAsync($"api/MbMTasks/toggle/{task.Id}", task);
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "Error al cambiar estado de la tarea.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        await LoadMbMAsync();
    }

    private void ConfirmDeleteTask(MbMActivity activity, MbMTask task)
    {
        TaskTargetActivity = activity;
        EditingTask = task;
        ConfirmMessage = $"¿Estás seguro de eliminar la tarea \"{task.Title}\"?";
        ConfirmType = "task";
        IsConfirmVisible = true;
    }

    private async Task DeleteTask()
    {
        if (EditingTask.Id <= 0) return;

        var response = await repository.DeleteAsync<MbMTask>($"api/MbMTasks/{EditingTask.Id}");
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "Error al eliminar la tarea.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
        }

        IsConfirmVisible = false;
        await LoadMbMAsync();
    }

    private string GetStatusColor(ActivityStatus status) => status switch
    {
        ActivityStatus.Completada => "success",
        ActivityStatus.EnProgreso => "warning",
        ActivityStatus.Pendiente => "secondary",
        ActivityStatus.Cancelada => "danger",
        _ => "secondary"
    };

    private string GetStatusColorBorder(ActivityStatus status) => status switch
    {
        ActivityStatus.Completada => "#198754",
        ActivityStatus.EnProgreso => "#ffc107",
        ActivityStatus.Pendiente => "#6c757d",
        ActivityStatus.Cancelada => "#dc3545",
        _ => "#6c757d"
    };

    private string GetPriorityColor(ActivityPriority priority) => priority switch
    {
        ActivityPriority.Alta => "danger",
        ActivityPriority.Media => "warning",
        ActivityPriority.Baja => "info",
        _ => "secondary"
    };

    private void ClearFilters()
    {
        FilterText = "";
        FilterStatus = "";
        FilterPriority = "";
    }

    private void HandleAddTask(MbMActivity activity) => OpenTaskModal(activity);
    private void HandleEditTask((MbMActivity Activity, MbMTask Task) args) => OpenTaskModal(args.Activity, args.Task);
    private void HandleDeleteTask((MbMActivity Activity, MbMTask Task) args) => ConfirmDeleteTask(args.Activity, args.Task);
    private void HandleToggleTask((MbMActivity Activity, MbMTask Task) args) => ToggleTask(args.Activity, args.Task);
    private void HandleAddProvider(MbMActivity activity) => OpenProviderModal(activity);
    private void HandleEditProvider((MbMActivity Activity, MbMProvider Provider) args) => OpenProviderModal(args.Activity, args.Provider);
    private void HandleDeleteProvider((MbMActivity Activity, MbMProvider Provider) args) => ConfirmDeleteProvider(args.Activity, args.Provider);
}

public class MinuteByMinuteContainer
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; }
    public List<MbMActivity>? Activities { get; set; }
}
