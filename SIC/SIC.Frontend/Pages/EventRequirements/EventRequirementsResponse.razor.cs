using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.EventRequirements;

[Authorize(Roles = "Admin")]
public partial class EventRequirementsResponse
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

    [Parameter] public string EventCode { get; set; } = "";

    private bool Loading = true;
    private bool HasError = false;
    private string? ErrorMessage;
    private string? EventName;

    private Dictionary<string, List<EventTypeRequirementDTO>>? Sections;
    private Dictionary<int, EventRequirementAnswer> AnswersByRequirement = new();
    private Dictionary<int, List<EventRequirementImage>> ImagesByRequirement = new();

    private bool IsLightboxVisible = false;
    private EventRequirementImage? SelectedImage;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        Loading = true;
        HasError = false;

        var eventResp = await repository.GetAsync<Event>($"api/Events/byCode/{EventCode}");
        if (eventResp.Error || eventResp.Response == null)
        {
            HasError = true;
            ErrorMessage = "No se encontró el evento.";
            Loading = false;
            return;
        }

        var ev = eventResp.Response;
        EventName = ev.Name;

        if (ev.EventTypeId == null)
        {
            HasError = true;
            ErrorMessage = "Este evento no tiene tipo asignado.";
            Loading = false;
            return;
        }

        var typeReqsResp = await repository.GetAsync<List<EventTypeRequirementDTO>>(
            $"api/EventTypeRequirements/byEventTypeId/{ev.EventTypeId}");
        if (typeReqsResp.Error || typeReqsResp.Response == null)
        {
            HasError = true;
            ErrorMessage = "No se pudieron cargar los requisitos del evento.";
            Loading = false;
            return;
        }

        var typeReqs = typeReqsResp.Response;

        var answersResp = await repository.GetAsync<List<EventRequirementAnswer>>(
            $"api/EventRequirementAnswers/byEventId/{ev.Id}");

        if (!answersResp.Error && answersResp.Response != null)
        {
            foreach (var answer in answersResp.Response)
            {
                AnswersByRequirement[answer.RequirementId] = answer;

                if (answer.Images != null && answer.Images.Count > 0)
                {
                    ImagesByRequirement[answer.RequirementId] = answer.Images.OrderBy(i => i.Order).ToList();
                }
            }
        }

        Sections = typeReqs
            .GroupBy(r => r.RequirementSection ?? "General")
            .ToDictionary(g => g.Key, g => g.ToList());

        Loading = false;
    }

    private string? GetAnswer(int requirementId)
    {
        return AnswersByRequirement.TryGetValue(requirementId, out var answer)
            ? answer.Value
            : null;
    }

    private List<EventRequirementImage>? GetImages(int requirementId)
    {
        return ImagesByRequirement.TryGetValue(requirementId, out var images)
            ? images
            : null;
    }

    private void OpenLightbox(EventRequirementImage image)
    {
        SelectedImage = image;
        IsLightboxVisible = true;
    }

    private void CloseLightbox()
    {
        IsLightboxVisible = false;
        SelectedImage = null;
    }

    private async Task DeleteImage(EventRequirementImage img)
    {
        var result = await sweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = "¿Eliminar imagen?",
            Text = $"Se eliminará \"{img.OriginalName}\". Esta acción no se puede deshacer.",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Sí, eliminar",
            CancelButtonText = "Cancelar"
        });

        if (string.IsNullOrEmpty(result.Value)) return;

        var response = await repository.DeleteAsync<EventRequirementImage>($"api/EventRequirementImages/{img.Id}");
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "No se pudo eliminar la imagen.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        foreach (var reqId in ImagesByRequirement.Keys.ToList())
        {
            var list = ImagesByRequirement[reqId];
            if (list.Remove(img))
            {
                if (list.Count == 0) ImagesByRequirement.Remove(reqId);
                break;
            }
        }

        StateHasChanged();
        await sweetAlertService.FireAsync("Eliminada", "Imagen eliminada correctamente.", SweetAlertIcon.Success);
    }
}
