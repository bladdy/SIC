using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.EventRequirements;

[Authorize(Roles = "Admin")]
public partial class EventRequirementsIndex
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

    public List<EventType>? EventTypes { get; set; }
    private Dictionary<int, int> RequirementCounts { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        var typesResponse = await repository.GetAsync<List<EventType>>("api/EventTypes");
        if (typesResponse.Error)
        {
            var message = await typesResponse.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        EventTypes = typesResponse.Response;

        if (EventTypes != null)
        {
            var countTasks = EventTypes.Select(async et =>
            {
                var resp = await repository.GetAsync<List<SIC.Shared.DTOs.EventTypeRequirementDTO>>(
                    $"api/EventTypeRequirements/byEventTypeId/{et.Id}");
                if (!resp.Error && resp.Response != null)
                    return (et.Id, resp.Response.Count());
                return (et.Id, 0);
            });

            var results = await Task.WhenAll(countTasks);
            RequirementCounts = results.ToDictionary(r => r.Id, r => r.Item2);
        }
    }
}
