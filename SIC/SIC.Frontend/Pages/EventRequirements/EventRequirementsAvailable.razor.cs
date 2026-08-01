using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

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
        IsEditMode = false;
        IsModalVisible = true;
    }

    private void ShowEditModal(EventRequirement req)
    {
        NewRequirement = new EventRequirement
        {
            Id = req.Id,
            Name = req.Name,
            Description = req.Description,
            Section = req.Section,
            InputType = req.InputType,
            Placeholder = req.Placeholder,
            IsRequired = req.IsRequired,
            MinImages = req.MinImages,
            MaxImages = req.MaxImages,
            SortOrder = req.SortOrder,
            IsActive = req.IsActive
        };
        IsEditMode = true;
        IsModalVisible = true;
    }

    private void CloseModal()
    {
        IsModalVisible = false;
    }

    private async Task SaveRequirement()
    {
        HttpResponseWrapper<object>? responseHttp;

        if (IsEditMode)
        {
            responseHttp = await repository.PutAsync("api/EventRequirements", NewRequirement);
        }
        else
        {
            responseHttp = await repository.PostAsync("api/EventRequirements", NewRequirement);
        }

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el requisito.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
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

        await LoadRequirements();
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
