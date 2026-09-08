using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.Net.Http.Headers;

namespace SIC.Frontend.Pages.EventRequirements;

[Authorize(Roles = "Admin")]
public partial class EventRequirementsAvailable
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

    public List<EventRequirement>? Requirements { get; set; }
    private EventRequirement NewRequirement = new();
    private bool IsModalVisible = false;
    private bool IsEditMode = false;
    private bool Saving = false;

    private List<RequirementImageOptionDTO> ImageOptions = new();
    private Dictionary<RequirementImageOptionDTO, PendingOptionImage> PendingOptionImages = new();
    private record PendingOptionImage(byte[] Data, string FileName);

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadRequirements();
    }

    private async Task LoadRequirements()
    {
        var responseHttp = await repository.GetAsync<List<EventRequirement>>("api/EventRequirements");
        Requirements = responseHttp.Response;
    }

    private void ShowCreateModal()
    {
        NewRequirement = new EventRequirement
        {
            IsActive = true,
            SortOrder = (Requirements?.Count ?? 0) + 1
        };
        ImageOptions = new();
        PendingOptionImages = new();
        IsEditMode = false;
        IsModalVisible = true;
    }

    private async Task ShowEditModal(EventRequirement req)
    {
        NewRequirement = new EventRequirement
        {
            Id = req.Id,
            Name = req.Name,
            Description = req.Description,
            Section = req.Section,
            InputType = req.InputType,
            Placeholder = req.Placeholder,
            ButtonName = req.ButtonName,
            ButtonUrl = req.ButtonUrl,
            IsRequired = req.IsRequired,
            MinImages = req.MinImages,
            MaxImages = req.MaxImages,
            SortOrder = req.SortOrder,
            IsActive = req.IsActive
        };

        ImageOptions = new();
        PendingOptionImages = new();

        if (req.InputType == RequirementInputType.ImagenConTitulo)
        {
            var resp = await repository.GetAsync<List<RequirementImageOption>>(
                $"api/RequirementImageOptions/byRequirement/{req.Id}");
            if (!resp.Error && resp.Response != null)
            {
                ImageOptions = resp.Response.Select(o => new RequirementImageOptionDTO
                {
                    Id = o.Id,
                    RequirementId = o.RequirementId,
                    Title = o.Title,
                    ImageUrl = o.ImageUrl,
                    SortOrder = o.SortOrder
                }).ToList();
            }
        }

        IsEditMode = true;
        IsModalVisible = true;
    }

    private void CloseModal()
    {
        IsModalVisible = false;
    }

    private void AddImageOption()
    {
        ImageOptions.Add(new RequirementImageOptionDTO
        {
            SortOrder = ImageOptions.Count + 1
        });
    }

    private void RemoveImageOption(int index)
    {
        if (index < 0 || index >= ImageOptions.Count) return;

        var option = ImageOptions[index];
        ImageOptions.RemoveAt(index);
        PendingOptionImages.Remove(option);

        for (int i = 0; i < ImageOptions.Count; i++)
            ImageOptions[i].SortOrder = i + 1;
    }

    private async Task HandleOptionImageUpload(int index, InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null || file.Size == 0) return;

        if (file.Size > 2 * 1024 * 1024)
        {
            await sweetAlertService.FireAsync("Imagen demasiado grande",
                $"\"{file.Name}\" supera el límite de 2 MB.", SweetAlertIcon.Warning);
            return;
        }

        using var stream = file.OpenReadStream(maxAllowedSize: 2 * 1024 * 1024);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var bytes = memoryStream.ToArray();

        if (index >= 0 && index < ImageOptions.Count)
        {
            var option = ImageOptions[index];
            PendingOptionImages[option] = new PendingOptionImage(bytes, file.Name);
            option.ImageUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
        }

        StateHasChanged();
    }

    private void UpdateOptionTitle(int index, string? value)
    {
        if (index >= 0 && index < ImageOptions.Count)
            ImageOptions[index].Title = value ?? "";
    }

    private bool IsOptionImageLocal(int index)
    {
        if (index < 0 || index >= ImageOptions.Count) return false;
        return PendingOptionImages.ContainsKey(ImageOptions[index]);
    }

    private async Task SaveRequirement()
    {
        if (NewRequirement.InputType == RequirementInputType.ImagenConTitulo && ImageOptions.Count == 0)
        {
            await sweetAlertService.FireAsync("Sin opciones",
                "Debe agregar al menos una opción de imagen con su título.",
                SweetAlertIcon.Warning);
            return;
        }

        Saving = true;

        HttpResponseWrapper<EventRequirement>? responseHttp;

        if (IsEditMode)
        {
            responseHttp = await repository.PutAsync<EventRequirement, EventRequirement>("api/EventRequirements", NewRequirement);
        }
        else
        {
            responseHttp = await repository.PostAsync<EventRequirement, EventRequirement>("api/EventRequirements", NewRequirement);
        }

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el requisito.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            Saving = false;
            return;
        }

        if (responseHttp.Response != null)
            NewRequirement = responseHttp.Response;

        if (NewRequirement.InputType == RequirementInputType.ImagenConTitulo)
        {
            var optionsSaved = await SaveImageOptions();
            if (!optionsSaved)
                return;
        }

        CloseModal();

        var toast = sweetAlertService.Mixin(new SweetAlertOptions
        {
            Toast = true,
            Position = SweetAlertPosition.TopEnd,
            ShowConfirmButton = false,
            Timer = 3000,
            TimerProgressBar = true,
        });
        await toast.FireAsync(
            "Éxito",
            IsEditMode ? "Requisito actualizado con éxito." : "Requisito creado con éxito.",
            SweetAlertIcon.Success
        );

        Saving = false;

        await LoadRequirements();
    }

    private async Task<bool> SaveImageOptions()
    {
        var content = new MultipartFormDataContent();
        var optionsToSend = new List<RequirementImageOptionDTO>();

        for (int i = 0; i < ImageOptions.Count; i++)
        {
            optionsToSend.Add(new RequirementImageOptionDTO
            {
                Title = ImageOptions[i].Title,
                ImageUrl = ImageOptions[i].ImageUrl,
                SortOrder = i + 1
            });

            if (PendingOptionImages.TryGetValue(ImageOptions[i], out var pending))
            {
                var fileContent = new StreamContent(new MemoryStream(pending.Data));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                content.Add(fileContent, $"option_{i}", pending.FileName);
            }
        }

        content.Add(new StringContent(
            System.Text.Json.JsonSerializer.Serialize(optionsToSend),
            System.Text.Encoding.UTF8, "application/json"), "options");

        var response = await repository.PostMultipartAsync<object>(
            $"api/RequirementImageOptions/save/{NewRequirement.Id}", content);

        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "No se pudieron guardar las opciones.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            Saving = false;
            return false;
        }

        return true;
    }

    private async Task ConfirmDelete(EventRequirement req)
    {
        var result = await sweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = "¿Está seguro?",
            Text = $"Se eliminará el requisito '{req.Name}'. Esta acción no se puede deshacer.",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Sí, borrar",
            CancelButtonText = "Cancelar"
        });

        if (!string.IsNullOrEmpty(result.Value))
        {
            await DeleteRequirement(req);
        }
    }

    private async Task DeleteRequirement(EventRequirement req)
    {
        var responseHttp = await repository.DeleteAsync<EventRequirement>($"api/EventRequirements/{req.Id}");

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el requisito.";
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
        await toast.FireAsync("Eliminado", "El requisito fue borrado correctamente.", SweetAlertIcon.Success);

        await LoadRequirements();
    }
}