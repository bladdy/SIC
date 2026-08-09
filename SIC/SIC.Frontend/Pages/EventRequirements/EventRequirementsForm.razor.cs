using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SIC.Frontend.Pages.EventRequirements;

public partial class EventRequirementsForm
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public string EventCode { get; set; } = "";

    private bool Loading = true;
    private bool Saving = false;
    private bool SubmitAttempted = false;
    private EventRequirementFormDTO? FormDTO;
    private Dictionary<int, string?> AnswerValues { get; set; } = new();
    private Dictionary<int, List<EventRequirementImageDTO>> ImagesByRequirement { get; set; } = new();
    private Dictionary<string, List<EventTypeRequirementDTO>> Sections { get; set; } = new();
    private HashSet<int> FailedFields { get; set; } = new();
    private Dictionary<int, string> FieldErrorMessages { get; set; } = new();
    private Dictionary<int, List<PendingImage>> PendingImages { get; set; } = new();
    private HashSet<int> LockedRequirementIds { get; set; } = new();

    private record PendingImage(byte[] Data, string FileName);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadForm();
    }

    private async Task LoadForm()
    {
        Loading = true;

        var eventResp = await repository.GetAsync<Event>($"api/Events/byCode/{EventCode}");
        if (eventResp.Error || eventResp.Response == null)
        {
            await sweetAlertService.FireAsync("Error", "No se encontró el evento.", SweetAlertIcon.Error);
            Loading = false;
            return;
        }

        var ev = eventResp.Response;
        if (ev.EventTypeId == null)
        {
            await sweetAlertService.FireAsync("Info", "Este evento no tiene tipo asignado.", SweetAlertIcon.Info);
            Loading = false;
            return;
        }

        var typeReqsResp = await repository.GetAsync<List<EventTypeRequirementDTO>>(
            $"api/EventTypeRequirements/byEventTypeId/{ev.EventTypeId}");
        if (typeReqsResp.Error || typeReqsResp.Response == null || !typeReqsResp.Response.Any())
        {
            Loading = false;
            return;
        }

        var typeReqs = typeReqsResp.Response.ToList();

        var answersResp = await repository.GetAsync<List<EventRequirementAnswer>>(
            $"api/EventRequirementAnswers/byEventId/{ev.Id}");
        var existingAnswers = !answersResp.Error && answersResp.Response != null
            ? answersResp.Response.ToList()
            : new List<EventRequirementAnswer>();

        var imagesResp = await repository.GetAsync<List<EventRequirementImage>>(
            $"api/EventRequirementImages/byEventId/{ev.Id}");
        var existingImages = !imagesResp.Error && imagesResp.Response != null
            ? imagesResp.Response.ToList()
            : new List<EventRequirementImage>();

        FormDTO = new EventRequirementFormDTO
        {
            EventId = ev.Id,
            EventTypeId = ev.EventTypeId.Value,
            EventName = ev.Name,
            EventTypeName = ev.EventType?.Name,
            Requirements = typeReqs,
            Answers = existingAnswers.Select(a => new EventRequirementAnswerDTO
            {
                Id = a.Id,
                EventId = a.EventId,
                RequirementId = a.RequirementId,
                Value = a.Value
            }).ToList()
        };

        AnswerValues = typeReqs.ToDictionary(
            r => r.RequirementId,
            r => existingAnswers.FirstOrDefault(a => a.RequirementId == r.RequirementId)?.Value);

        ImagesByRequirement = typeReqs
            .Where(r => r.RequirementInputType == RequirementInputType.Image)
            .ToDictionary(
                r => r.RequirementId,
                r => existingImages
                    .Where(img => existingAnswers.Any(a => a.Id == img.RequirementAnswerId && a.RequirementId == r.RequirementId))
                    .Select(img => new EventRequirementImageDTO
                    {
                        Id = img.Id,
                        RequirementAnswerId = img.RequirementAnswerId,
                        FileName = img.FileName,
                        OriginalName = img.OriginalName,
                        Path = img.Path,
                        Order = img.Order
                    })
                    .ToList());

        Sections = typeReqs
            .GroupBy(r => r.RequirementSection ?? "General")
            .ToDictionary(g => g.Key, g => g.ToList());

        LockedRequirementIds = typeReqs
            .Where(r =>
            {
                if (r.RequirementInputType == RequirementInputType.Image)
                    return false;

                var value = existingAnswers.FirstOrDefault(a => a.RequirementId == r.RequirementId)?.Value;
                return !string.IsNullOrWhiteSpace(value);
            })
            .Select(r => r.RequirementId)
            .ToHashSet();

        Loading = false;
    }

    private EventRequirementAnswerDTO? GetAnswer(int requirementId)
    {
        return FormDTO?.Answers.FirstOrDefault(a => a.RequirementId == requirementId);
    }

    private void SetAnswer(int requirementId, string? value)
    {
        AnswerValues[requirementId] = value;
        FailedFields.Remove(requirementId);
        FieldErrorMessages.Remove(requirementId);

        if (FormDTO == null) return;

        var existing = FormDTO.Answers.FirstOrDefault(a => a.RequirementId == requirementId);
        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            FormDTO.Answers.Add(new EventRequirementAnswerDTO
            {
                EventId = FormDTO.EventId,
                RequirementId = requirementId,
                Value = value
            });
        }
    }

    private string GetInputClass(int requirementId)
    {
        return SubmitAttempted && FailedFields.Contains(requirementId) ? "form-control is-invalid" : "form-control";
    }

    private EventTypeRequirementDTO? GetRequirement(int requirementId)
    {
        return FormDTO?.Requirements.FirstOrDefault(r => r.RequirementId == requirementId);
    }

    private int GetImageCount(int requirementId)
    {
        return ImagesByRequirement.TryGetValue(requirementId, out var imgs) ? imgs.Count : 0;
    }

    private bool IsFieldLocked(int requirementId)
    {
        return LockedRequirementIds.Contains(requirementId);
    }

    private async Task HandleImageUpload(int requirementId, InputFileChangeEventArgs e)
    {
        var files = e.GetMultipleFiles();
        if (files.Count == 0) return;

        var req = GetRequirement(requirementId);
        if (req == null) return;

        var currentCount = GetImageCount(requirementId);
        var max = req.RequirementMaxImages;
        if (max > 0 && currentCount >= max)
        {
            await sweetAlertService.FireAsync("Límite alcanzado",
                max == 1
                    ? "Este campo solo admite 1 imagen."
                    : $"Este campo solo admite hasta {max} imágenes.",
                SweetAlertIcon.Warning);
            return;
        }

        var remaining = max > 0 ? max - currentCount : files.Count;
        if (remaining <= 0) return;

        foreach (var file in files.Take(remaining))
        {
            if (file == null || file.Size == 0) continue;
            await AddPendingImage(requirementId, file);
        }

        FailedFields.Remove(requirementId);
        FieldErrorMessages.Remove(requirementId);
        StateHasChanged();
    }

    private async Task AddPendingImage(int requirementId, IBrowserFile file)
    {
        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        var base64 = Convert.ToBase64String(bytes);
        var previewUrl = $"data:{file.ContentType};base64,{base64}";

        if (!ImagesByRequirement.ContainsKey(requirementId))
            ImagesByRequirement[requirementId] = new List<EventRequirementImageDTO>();
        if (!PendingImages.ContainsKey(requirementId))
            PendingImages[requirementId] = new List<PendingImage>();

        PendingImages[requirementId].Add(new PendingImage(bytes, file.Name));
        ImagesByRequirement[requirementId].Add(new EventRequirementImageDTO
        {
            Id = 0,
            OriginalName = file.Name,
            FileName = file.Name,
            Path = previewUrl,
            Order = ImagesByRequirement[requirementId].Count + 1
        });
    }

    private void RemoveImage(int requirementId, EventRequirementImageDTO img)
    {
        if (!ImagesByRequirement.TryGetValue(requirementId, out var list)) return;

        list.Remove(img);

        if (img.Id == 0 && PendingImages.TryGetValue(requirementId, out var pending))
        {
            var match = pending.FindIndex(p => p.FileName == img.OriginalName);
            if (match >= 0) pending.RemoveAt(match);
            if (pending.Count == 0) PendingImages.Remove(requirementId);
        }

        if (list.Count == 0) ImagesByRequirement.Remove(requirementId);

        FailedFields.Remove(requirementId);
        FieldErrorMessages.Remove(requirementId);
        StateHasChanged();
    }

    private async Task SaveAll()
    {
        if (FormDTO == null) return;

        SubmitAttempted = true;
        FailedFields.Clear();
        FieldErrorMessages.Clear();

        foreach (var req in FormDTO.Requirements)
        {
            if (req.RequirementInputType == RequirementInputType.Image)
            {
                var count = GetImageCount(req.RequirementId);
                var min = req.RequirementMinImages > 0
                    ? req.RequirementMinImages
                    : (req.RequirementIsRequired == true ? 1 : 0);
                var max = req.RequirementMaxImages;

                if (max > 0 && count > max)
                {
                    FailedFields.Add(req.RequirementId);
                    FieldErrorMessages[req.RequirementId] = max == 1
                        ? "Este campo solo admite 1 imagen."
                        : $"Este campo solo admite hasta {max} imágenes.";
                }
                else if (count < min)
                {
                    FailedFields.Add(req.RequirementId);
                    FieldErrorMessages[req.RequirementId] = min == 1
                        ? "Debes subir al menos 1 imagen."
                        : $"Debes subir al menos {min} imágenes.";
                }
            }
            else if (req.RequirementIsRequired == true)
            {
                var val = AnswerValues.GetValueOrDefault(req.RequirementId);
                if (string.IsNullOrWhiteSpace(val))
                {
                    FailedFields.Add(req.RequirementId);
                    FieldErrorMessages[req.RequirementId] = "Este campo es obligatorio.";
                }
            }
        }

        if (FailedFields.Count > 0)
        {
            await sweetAlertService.FireAsync("Validación",
                "Por favor corrige los campos marcados en rojo.",
                SweetAlertIcon.Warning);
            return;
        }

        Saving = true;

        var allAnswers = new List<EventRequirementAnswerDTO>();
        foreach (var req in FormDTO.Requirements)
        {
            allAnswers.Add(new EventRequirementAnswerDTO
            {
                EventId = FormDTO.EventId,
                RequirementId = req.RequirementId,
                Value = req.RequirementInputType == RequirementInputType.Image
                    ? ""
                    : AnswerValues.GetValueOrDefault(req.RequirementId)
            });
        }

        var content = new MultipartFormDataContent();
        content.Add(new StringContent(FormDTO.EventId.ToString()), "eventId");
        content.Add(new StringContent(JsonSerializer.Serialize(allAnswers), Encoding.UTF8, "application/json"), "answers");

        var existingImages = ImagesByRequirement
            .SelectMany(kv => kv.Value
                .Where(i => i.Id != 0)
                .Select(i => new EventRequirementImageDTO
                {
                    Id = i.Id,
                    RequirementId = kv.Key,
                    RequirementAnswerId = i.RequirementAnswerId,
                    FileName = i.FileName,
                    OriginalName = i.OriginalName,
                    Path = i.Path,
                    Order = i.Order
                }))
            .ToList();
        content.Add(new StringContent(JsonSerializer.Serialize(existingImages), Encoding.UTF8, "application/json"), "existingImages");

        foreach (var (reqId, files) in PendingImages)
        {
            if (!ImagesByRequirement.TryGetValue(reqId, out var imgs)) continue;
            var pendingIndex = 0;
            foreach (var img in imgs.Where(i => i.Id == 0).OrderBy(i => i.Order))
            {
                if (pendingIndex >= files.Count) break;
                var pending = files[pendingIndex];
                var fileContent = new StreamContent(new MemoryStream(pending.Data));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, $"images_{reqId}_{img.Order}", pending.FileName);
                pendingIndex++;
            }
        }

        var response = await repository.PostMultipartAsync<SaveFormResponseDTO>(
            "api/EventRequirementAnswers/save-form", content);

        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message ?? "No se pudo guardar.", SweetAlertIcon.Error);
            Saving = false;
            return;
        }

        if (response.Response != null)
        {
            ImagesByRequirement = response.Response.Images
                .GroupBy(i => i.RequirementId)
                .ToDictionary(g => g.Key, g => g.ToList());
            PendingImages.Clear();
        }

        foreach (var req in FormDTO.Requirements)
        {
            if (req.RequirementInputType == RequirementInputType.Image)
                continue;

            var hasContent = !string.IsNullOrWhiteSpace(AnswerValues.GetValueOrDefault(req.RequirementId));
            if (hasContent) LockedRequirementIds.Add(req.RequirementId);
        }

        await sweetAlertService.Mixin(new SweetAlertOptions
        {
            Toast = true,
            Position = SweetAlertPosition.TopEnd,
            ShowConfirmButton = false,
            Timer = 3000,
            TimerProgressBar = true
        }).FireAsync("Guardado", "Tu información ha sido guardada exitosamente.", SweetAlertIcon.Success);

        Saving = false;
    }
}