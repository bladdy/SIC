using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Pages.EventRequirements;

[Authorize(Roles = "Admin")]
public partial class EventTypeRequirementsDetail
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public int EventTypeId { get; set; }

    private string EventTypeName = "";
    private bool Loading = true;
    private string SearchTerm = "";

    private List<EventTypeRequirementDTO> AssignedRequirements { get; set; } = new();
    private List<EventRequirement> AllRequirements { get; set; } = new();
    private HashSet<int> AssignedRequirementIds { get; set; } = new();

    private IEnumerable<EventRequirement> FilteredAvailableRequirements =>
        string.IsNullOrWhiteSpace(SearchTerm)
            ? AllRequirements.Where(r => !AssignedRequirementIds.Contains(r.Id))
            : AllRequirements.Where(r => !AssignedRequirementIds.Contains(r.Id) &&
                (r.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                 r.Section.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)));

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        Loading = true;

        // Load event type name
        var typeResp = await repository.GetAsync<EventType>($"api/EventTypes/{EventTypeId}");
        if (!typeResp.Error && typeResp.Response != null)
            EventTypeName = typeResp.Response.Name;

        // Load assigned requirements
        var assignedResp = await repository.GetAsync<List<EventTypeRequirementDTO>>(
            $"api/EventTypeRequirements/byEventTypeId/{EventTypeId}");
        if (!assignedResp.Error && assignedResp.Response != null)
        {
            AssignedRequirements = assignedResp.Response.OrderBy(r => r.SortOrder).ToList();
            AssignedRequirementIds = AssignedRequirements.Select(a => a.RequirementId).ToHashSet();
        }

        // Load all requirements
        var allResp = await repository.GetAsync<List<EventRequirement>>("api/EventRequirements");
        if (!allResp.Error && allResp.Response != null)
        {
            AllRequirements = allResp.Response.OrderBy(r => r.Section).ThenBy(r => r.SortOrder).ToList();
        }

        Loading = false;
    }

    private async Task AddRequirement(EventRequirement requirement)
    {
        var dto = new EventTypeRequirementDTO
        {
            EventTypeId = EventTypeId,
            RequirementId = requirement.Id,
            SortOrder = AssignedRequirements.Count > 0 ? AssignedRequirements.Max(a => a.SortOrder) + 1 : 1
        };

        var response = await repository.PostAsync("api/EventTypeRequirements", dto);
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        await sweetAlertService.Mixin(new SweetAlertOptions
        {
            Toast = true, Position = SweetAlertPosition.TopEnd,
            ShowConfirmButton = false, Timer = 2000, TimerProgressBar = true
        }).FireAsync("Agregado", $"'{requirement.Name}' agregado.", SweetAlertIcon.Success);

        await LoadData();
    }

    private async Task RemoveRequirement(EventTypeRequirementDTO item)
    {
        var result = await sweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = "Remover requisito?",
            Text = $"Se eliminará '{item.RequirementName}' de este tipo de evento.",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Sí, remover",
            CancelButtonText = "Cancelar"
        });

        if (string.IsNullOrEmpty(result.Value)) return;

        var response = await repository.DeleteAsync<EventTypeRequirement>($"api/EventTypeRequirements/{item.Id}");
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        await LoadData();
    }

    private void MoveUp(EventTypeRequirementDTO item)
    {
        var idx = AssignedRequirements.FindIndex(a => a.Id == item.Id);
        if (idx <= 0) return;
        var prev = AssignedRequirements[idx - 1];
        int temp = item.SortOrder;
        item.SortOrder = prev.SortOrder;
        prev.SortOrder = temp;
        AssignedRequirements.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
    }

    private void MoveDown(EventTypeRequirementDTO item)
    {
        var idx = AssignedRequirements.FindIndex(a => a.Id == item.Id);
        if (idx < 0 || idx >= AssignedRequirements.Count - 1) return;
        var next = AssignedRequirements[idx + 1];
        int temp = item.SortOrder;
        item.SortOrder = next.SortOrder;
        next.SortOrder = temp;
        AssignedRequirements.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
    }

    private async Task SaveOrder()
    {
        foreach (var item in AssignedRequirements)
        {
            var dto = new EventTypeRequirementDTO
            {
                Id = item.Id,
                EventTypeId = EventTypeId,
                RequirementId = item.RequirementId,
                SortOrder = item.SortOrder
            };
            var response = await repository.PutAsync("api/EventTypeRequirements", dto);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
        }

        await sweetAlertService.Mixin(new SweetAlertOptions
        {
            Toast = true, Position = SweetAlertPosition.TopEnd,
            ShowConfirmButton = false, Timer = 2000, TimerProgressBar = true
        }).FireAsync("Guardado", "El orden se ha actualizado.", SweetAlertIcon.Success);
    }
}
