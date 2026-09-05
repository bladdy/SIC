using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Response;

namespace SIC.Frontend.Pages.Auth
{
    public partial class RecoverPassword
    {
        private RecoverPasswordDTO recoverPasswordDTO { get; set; } = new();
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService Swal { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;

        private async Task RecoverAsync()
        {
            var response = await Repository.PostAsync<RecoverPasswordDTO, ActionResponse<string>>("api/Accounts/RecoverPassword", recoverPasswordDTO);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await Swal.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await Swal.FireAsync("Solicitud enviada", response.Response!.Message ?? "Revisa tu correo para continuar con el restablecimiento.", SweetAlertIcon.Success);
            NavigationManager.NavigateTo("/login");
        }
    }
}