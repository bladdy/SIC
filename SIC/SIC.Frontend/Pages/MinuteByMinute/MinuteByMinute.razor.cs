using Microsoft.AspNetCore.Components;

namespace SIC.Frontend.Pages.MinuteByMinute;

public enum ActivityStatus
{
    Pendiente,
    EnProgreso,
    Completada,
    Cancelada
}

public enum Priority
{
    Alta,
    Media,
    Baja
}

public class MbMActivity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Pendiente;
    public Priority Priority { get; set; } = Priority.Media;
    public string Location { get; set; } = "";
    public string Notes { get; set; } = "";
    public List<MbMProvider> Providers { get; set; } = new();
    public List<MbMTask> Tasks { get; set; } = new();
}

public class MbMProvider
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Service { get; set; } = "";
    public string Status { get; set; } = "Confirmado";
    public decimal? Cost { get; set; }
}

public class MbMTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = "";
    public bool IsCompleted { get; set; }
    public string AssignedTo { get; set; } = "";
    public DateTime? DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Media;
}

public partial class MinuteByMinute
{
    [Parameter] public string? EventId { get; set; }

    private string EventName = "Boda de Fernanda & Raul";
    private string ActiveTab = "timeline";

    private List<MbMActivity> Activities = new();
    private List<MbMActivity> FilteredActivities => Activities
        .Where(a =>
            (string.IsNullOrWhiteSpace(FilterText) || a.Title.Contains(FilterText, StringComparison.OrdinalIgnoreCase) || a.Description.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
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

    protected override void OnInitialized()
    {
        LoadMockData();
    }

    private void LoadMockData()
    {
        var eventDate = new DateTime(2026, 8, 15);

        Activities = new List<MbMActivity>
        {
            new()
            {
                Title = "Preparacion del salon",
                Description = "Montaje de mesas, sillas, manteleria y decoracion basica del salon de eventos.",
                StartTime = eventDate.AddHours(8),
                EndTime = eventDate.AddHours(11),
                Status = ActivityStatus.Completada,
                Priority = Priority.Alta,
                Location = "Salon Principal - Jardin del Valle",
                Notes = "Confirmar aforo maximo de 200 personas.",
                Providers = new()
                {
                    new() { Name = "Decoraciones El Paraiso", Contact = "555-0101", Service = "Mobiliario y manteleria", Status = "Confirmado", Cost = 15000 },
                    new() { Name = "Limpieza Express", Contact = "555-0102", Service = "Limpieza general", Status = "Confirmado", Cost = 3000 }
                },
                Tasks = new()
                {
                    new() { Title = "Verificar aforo", IsCompleted = true, AssignedTo = "Carlos (Planner)", DueDate = eventDate.AddDays(-1), Priority = Priority.Alta },
                    new() { Title = "Coordinar entrada de mobiliario", IsCompleted = true, AssignedTo = "Carlos (Planner)", DueDate = eventDate.AddHours(7), Priority = Priority.Alta },
                    new() { Title = "Revisar iluminacion", IsCompleted = true, AssignedTo = "Tecnico de iluminacion", DueDate = eventDate.AddHours(9), Priority = Priority.Media }
                }
            },
            new()
            {
                Title = "Llegada de proveedor de flores",
                Description = "Arreglo de centros de mesa, arco floral para la ceremonia y decoracion de entradas.",
                StartTime = eventDate.AddHours(10),
                EndTime = eventDate.AddHours(13),
                Status = ActivityStatus.EnProgreso,
                Priority = Priority.Alta,
                Location = "Salon Principal y Capilla",
                Notes = "Flores deben mantenerse en refrigerador hasta 2 horas antes.",
                Providers = new()
                {
                    new() { Name = "Flores & Arte", Contact = "555-0201", Service = "Decoracion floral", Status = "Confirmado", Cost = 25000 },
                    new() { Name = "Refrigerados SA", Contact = "555-0202", Service = "Transporte refrigerado", Status = "Pendiente", Cost = 2000 }
                },
                Tasks = new()
                {
                    new() { Title = "Recibir arreglos de centros de mesa", IsCompleted = true, AssignedTo = "Maria (Asistente)", DueDate = eventDate.AddHours(10), Priority = Priority.Alta },
                    new() { Title = "Montar arco floral en capilla", IsCompleted = false, AssignedTo = "Flores & Arte", DueDate = eventDate.AddHours(12), Priority = Priority.Alta },
                    new() { Title = "Decorar mesas de invitados", IsCompleted = false, AssignedTo = "Flores & Arte", DueDate = eventDate.AddHours(13), Priority = Priority.Media }
                }
            },
            new()
            {
                Title = "Ensayo de la ceremonia",
                Description = "Ensayo general de la ceremonia con padrinos, novios y oficiante.",
                StartTime = eventDate.AddHours(14),
                EndTime = eventDate.AddHours(15),
                Status = ActivityStatus.Pendiente,
                Priority = Priority.Media,
                Location = "Capilla de San Jose",
                Notes = "Todos los participantes deben estar vestidos.",
                Providers = new(),
                Tasks = new()
                {
                    new() { Title = "Confirmar asistencia del oficiante", IsCompleted = false, AssignedTo = "Carlos (Planner)", DueDate = eventDate.AddHours(13), Priority = Priority.Alta },
                    new() { Title = "Preparar musica de entrada", IsCompleted = false, AssignedTo = "DJ Martinez", DueDate = eventDate.AddHours(13), Priority = Priority.Media }
                }
            },
            new()
            {
                Title = "Ceremonia principal",
                Description = "Ceremonia religiosa de union. Duracion estimada: 45 minutos.",
                StartTime = eventDate.AddHours(16),
                EndTime = eventDate.AddHours(17),
                Status = ActivityStatus.Pendiente,
                Priority = Priority.Alta,
                Location = "Capilla de San Jose",
                Notes = "No se permite flash durante la ceremonia.",
                Providers = new()
                {
                    new() { Name = "FotoVideo Profesional", Contact = "555-0301", Service = "Fotografia y video", Status = "Confirmado", Cost = 35000 },
                    new() { Name = "Sonido Ambiental", Contact = "555-0302", Service = "Sonido para ceremonia", Status = "Confirmado", Cost = 8000 }
                },
                Tasks = new()
                {
                    new() { Title = "Configurar sonido en capilla", IsCompleted = false, AssignedTo = "Sonido Ambiental", DueDate = eventDate.AddHours(15), Priority = Priority.Alta },
                    new() { Title = "Coordinar fila de padrinos", IsCompleted = false, AssignedTo = "Maria (Asistente)", DueDate = eventDate.AddHours(15), Priority = Priority.Media },
                    new() { Title = "Preparar alianzas", IsCompleted = false, AssignedTo = "Padrino principal", DueDate = eventDate.AddHours(15), Priority = Priority.Alta }
                }
            },
            new()
            {
                Title = "Coctel de bienvenida",
                Description = "Aperitivos y bebidas para invitados mientras se prepara la recepcion.",
                StartTime = eventDate.AddHours(17),
                EndTime = eventDate.AddHours(18),
                Status = ActivityStatus.Pendiente,
                Priority = Priority.Media,
                Location = "Terraza del Salon",
                Notes = "Incluir opciones vegetarianas y sin gluten.",
                Providers = new()
                {
                    new() { Name = "Bebidas Premium SA", Contact = "555-0401", Service = "Barra de bebidas", Status = "Confirmado", Cost = 20000 }
                },
                Tasks = new()
                {
                    new() { Title = "Montar estacion de cocteles", IsCompleted = false, AssignedTo = "Bebidas Premium SA", DueDate = eventDate.AddHours(16), Priority = Priority.Alta },
                    new() { Title = "Preparar botana fria", IsCompleted = false, AssignedTo = "Catering La Maison", DueDate = eventDate.AddHours(16), Priority = Priority.Media }
                }
            },
            new()
            {
                Title = "Recepcion y cena",
                Description = "Cena formal con servicio de mesa. Menu: 3 tiempos.",
                StartTime = eventDate.AddHours(18),
                EndTime = eventDate.AddHours(21),
                Status = ActivityStatus.Pendiente,
                Priority = Priority.Alta,
                Location = "Salon Principal",
                Notes = "Coordinar servicio de mesa con el capitan de meseros.",
                Providers = new()
                {
                    new() { Name = "Catering La Maison", Contact = "555-0501", Service = "Catering y servicio de mesa", Status = "Confirmado", Cost = 85000 },
                    new() { Name = "Pasteleria Dulce Sueno", Contact = "555-0502", Service = "Pastel nupcial", Status = "Confirmado", Cost = 12000 }
                },
                Tasks = new()
                {
                    new() { Title = "Verificar menu final con catering", IsCompleted = false, AssignedTo = "Carlos (Planner)", DueDate = eventDate.AddDays(-2), Priority = Priority.Alta },
                    new() { Title = "Coordinar timing de cada tiempo", IsCompleted = false, AssignedTo = "Catering La Maison", DueDate = eventDate.AddHours(17), Priority = Priority.Alta },
                    new() { Title = "Preparar sala para recepcion", IsCompleted = false, AssignedTo = "Maria (Asistente)", DueDate = eventDate.AddHours(17), Priority = Priority.Media }
                }
            },
            new()
            {
                Title = "Primer baile y pista",
                Description = "Primer baile de los novios seguido de apertura de pista para todos los invitados.",
                StartTime = eventDate.AddHours(21),
                EndTime = eventDate.AddHours(23),
                Status = ActivityStatus.Pendiente,
                Priority = Priority.Baja,
                Location = "Pista de Baile - Salon Principal",
                Notes = "Cancion del primer baile: 'Perfect' - Ed Sheeran.",
                Providers = new()
                {
                    new() { Name = "DJ Martinez", Contact = "555-0601", Service = "Servicio de DJ", Status = "Confirmado", Cost = 15000 }
                },
                Tasks = new()
                {
                    new() { Title = "Confirmar lista de reproduccion", IsCompleted = false, AssignedTo = "DJ Martinez", DueDate = eventDate.AddDays(-1), Priority = Priority.Media },
                    new() { Title = "Preparar efectos de iluminacion", IsCompleted = false, AssignedTo = "Tecnico de iluminacion", DueDate = eventDate.AddHours(20), Priority = Priority.Baja }
                }
            },
            new()
            {
                Title = "Cierre del evento",
                Description = "Despedida de los novios, lanzamiento de arroz y cierre de barra.",
                StartTime = eventDate.AddHours(23),
                EndTime = eventDate.AddDays(1).AddHours(0),
                Status = ActivityStatus.Pendiente,
                Priority = Priority.Media,
                Location = "Entrada Principal del Salon",
                Notes = "Preparar bolsitas de confeti para invitados.",
                Providers = new(),
                Tasks = new()
                {
                    new() { Title = "Preparar zona de despedida", IsCompleted = false, AssignedTo = "Maria (Asistente)", DueDate = eventDate.AddHours(22), Priority = Priority.Media },
                    new() { Title = "Coordinar cierre de barra", IsCompleted = false, AssignedTo = "Bebidas Premium SA", DueDate = eventDate.AddHours(23), Priority = Priority.Alta },
                    new() { Title = "Verificar entrega de regalos", IsCompleted = false, AssignedTo = "Carlos (Planner)", DueDate = eventDate.AddDays(1), Priority = Priority.Baja }
                }
            }
        };
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

    private void SaveActivity()
    {
        if (IsActivityEditMode)
        {
            var existing = Activities.FirstOrDefault(a => a.Id == EditingActivity.Id);
            if (existing != null)
            {
                existing.Title = EditingActivity.Title;
                existing.Description = EditingActivity.Description;
                existing.StartTime = EditingActivity.StartTime;
                existing.EndTime = EditingActivity.EndTime;
                existing.Status = EditingActivity.Status;
                existing.Priority = EditingActivity.Priority;
                existing.Location = EditingActivity.Location;
                existing.Notes = EditingActivity.Notes;
            }
        }
        else
        {
            Activities.Add(EditingActivity);
        }
        IsActivityModalVisible = false;
    }

    private void ConfirmDeleteActivity(MbMActivity activity)
    {
        ConfirmTargetActivity = activity;
        IsConfirmVisible = true;
    }

    private void DeleteActivity()
    {
        if (ConfirmTargetActivity != null)
        {
            Activities.Remove(ConfirmTargetActivity);
            ConfirmTargetActivity = null;
        }
        IsConfirmVisible = false;
    }

    private void CloseConfirmModal() => IsConfirmVisible = false;

    private void OpenProviderModal(MbMActivity activity, MbMProvider? provider = null)
    {
        ProviderTargetActivity = activity;
        EditingProvider = provider != null
            ? new MbMProvider { Id = provider.Id, Name = provider.Name, Contact = provider.Contact, Service = provider.Service, Status = provider.Status, Cost = provider.Cost }
            : new MbMProvider();
        IsProviderModalVisible = true;
    }

    private void CloseProviderModal() => IsProviderModalVisible = false;

    private void SaveProvider()
    {
        if (ProviderTargetActivity == null) return;

        var existing = ProviderTargetActivity.Providers.FirstOrDefault(p => p.Id == EditingProvider.Id);
        if (existing != null)
        {
            existing.Name = EditingProvider.Name;
            existing.Contact = EditingProvider.Contact;
            existing.Service = EditingProvider.Service;
            existing.Status = EditingProvider.Status;
            existing.Cost = EditingProvider.Cost;
        }
        else
        {
            ProviderTargetActivity.Providers.Add(EditingProvider);
        }
        IsProviderModalVisible = false;
    }

    private void DeleteProvider(MbMActivity activity, MbMProvider provider)
    {
        activity.Providers.Remove(provider);
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

    private void SaveTask()
    {
        if (TaskTargetActivity == null) return;

        var existing = TaskTargetActivity.Tasks.FirstOrDefault(t => t.Id == EditingTask.Id);
        if (existing != null)
        {
            existing.Title = EditingTask.Title;
            existing.IsCompleted = EditingTask.IsCompleted;
            existing.AssignedTo = EditingTask.AssignedTo;
            existing.DueDate = EditingTask.DueDate;
            existing.Priority = EditingTask.Priority;
        }
        else
        {
            TaskTargetActivity.Tasks.Add(EditingTask);
        }
        IsTaskModalVisible = false;
    }

    private void ToggleTask(MbMActivity activity, MbMTask task)
    {
        task.IsCompleted = !task.IsCompleted;
    }

    private void DeleteTask(MbMActivity activity, MbMTask task)
    {
        activity.Tasks.Remove(task);
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

    private string GetPriorityColor(Priority priority) => priority switch
    {
        Priority.Alta => "danger",
        Priority.Media => "warning",
        Priority.Baja => "info",
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

    private void HandleDeleteTask((MbMActivity Activity, MbMTask Task) args) => DeleteTask(args.Activity, args.Task);

    private void HandleToggleTask((MbMActivity Activity, MbMTask Task) args) => ToggleTask(args.Activity, args.Task);

    private void HandleAddProvider(MbMActivity activity) => OpenProviderModal(activity);

    private void HandleEditProvider((MbMActivity Activity, MbMProvider Provider) args) => OpenProviderModal(args.Activity, args.Provider);

    private void HandleDeleteProvider((MbMActivity Activity, MbMProvider Provider) args) => DeleteProvider(args.Activity, args.Provider);
}
