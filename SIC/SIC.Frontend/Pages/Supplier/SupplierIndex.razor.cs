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

        // Cerrar el modal inmediatamente al confirmar que la operaci�n fue exitosa
        CloseModal();

        // Luego mostrar la notificaci�n
        await sweetAlertService.FireAsync(
            "�xito",
            IsEditMode ? "Proveedor actualizado con �xito." : "Proveedor creado con �xito.",
            SweetAlertIcon.Success
        );

        await LoadSuppliers();
    }

private async Task ConfirmDelete(SIC.Shared.Entities.Supplier supplier)
    {
        _pendingSupplier = supplier;
        ConfirmMessage = $"Se eliminará el Proveedor '{supplier.Name}'. Esta acción no se puede deshacer.";
        IsConfirmVisible = true;
        await Task.CompletedTask;
    }

    private bool IsConfirmVisible;
    private string ConfirmMessage = "";
    private SIC.Shared.Entities.Supplier? _pendingSupplier;

    private async Task OnConfirmVisibleChanged(bool visible)
    {
        IsConfirmVisible = visible;
        if (!visible) _pendingSupplier = null;
    }

    private async Task ExecutePendingDelete()
    {
        if (_pendingSupplier == null) return;
        await DeleteEventTypes(_pendingSupplier);
_pendingSupplier = null;
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

        await sweetAlertService.FireAsync("Eliminado", "El Proveedor fue borrado correctamente.", SweetAlertIcon.Success);

        await LoadSuppliers();
    }
}