using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Repositories;
using System.Security.Claims;

namespace SIC.Frontend.Pages.Supplier;

public partial class SupplierIndex
{
    private string? _userId;
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    public List<SIC.Shared.Entities.Supplier>? Supplier { get; set; }
    private SIC.Shared.Entities.Supplier NewSupplier = new();
    private bool IsModalVisible = false;
    private bool IsEditMode = false;  // Nuevo flag

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user.Identity is not null && user.Identity.IsAuthenticated)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _userId = userId;
            NewSupplier.UserId = userId ?? string.Empty;

            await LoadSuppliers();
        }
    }

    private async Task LoadSuppliers()
    {
        var responseHttp = await repository.GetAsync<List<SIC.Shared.Entities.Supplier>>($"api/Supplier/byUserId/{NewSupplier.UserId}");
        Supplier = responseHttp.Response;
    }

    private void ShowCreateModal()
    {
        NewSupplier = new SIC.Shared.Entities.Supplier();
        IsEditMode = false;
        IsModalVisible = true;
    }

    private void ShowEditModal(SIC.Shared.Entities.Supplier supplier)
    {
        // Clonar el objeto para no afectar la lista si cancelamos
        NewSupplier = new SIC.Shared.Entities.Supplier
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Company = supplier.Company,
            Email = supplier.Email,
            Mobile = supplier.Mobile,
            Notes = supplier.Notes,
            Phone = supplier.Phone
        };
        IsEditMode = true;
        IsModalVisible = true;
    }

    private void CloseModal()
    {
        IsModalVisible = false;
    }

    private async Task SaveEventTypes()
    {
        HttpResponseWrapper<object>? responseHttp;

        if (IsEditMode)
        {
            // PUT -> Editar
            responseHttp = await repository.PutAsync("api/Supplier", NewSupplier);
        }
        else
        {
            // POST -> Crear
            responseHttp = await repository.PostAsync<SIC.Shared.Entities.Supplier>("api/Supplier", NewSupplier);
        }

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el Proveedor";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        // Cerrar el modal inmediatamente al confirmar que la operación fue exitosa
        CloseModal();

        // Luego mostrar la notificación
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
            IsEditMode ? "Proveedor actualizado con éxito." : "Proveedor creado con éxito.",
            SweetAlertIcon.Success
        );

        await LoadSuppliers();
    }

    private async Task ConfirmDelete(SIC.Shared.Entities.Supplier supplier)
    {
        var result = await sweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = "¿Está seguro?",
            Text = $"Se eliminará el Proveedor '{supplier.Name}'. Esta acción no se puede deshacer.",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Sí, borrar",
            CancelButtonText = "Cancelar"
        });

        if (!string.IsNullOrEmpty(result.Value))
        {
            await DeleteEventTypes(supplier);
        }
    }

    private async Task DeleteEventTypes(SIC.Shared.Entities.Supplier supplier)
    {
        var responseHttp = await repository.DeleteAsync<SIC.Shared.Entities.Supplier>($"api/Supplier/{supplier.Id}");

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el Proveedor.";
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
        await toast.FireAsync("Eliminado", "El Proveedor fue borrado correctamente.", SweetAlertIcon.Success);

        await LoadSuppliers();
    }
}