using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Response;

namespace SIC.Frontend.Pages.Auth
{
    public partial class ResetPassword
    {
        private ResetPasswordDTO resetPasswordDTO { get; set; } = new();
        private string? confirmPassword;

        [SupplyParameterFromQuery(Name = "phone")]
        public string? Phone { get; set; }

        [SupplyParameterFromQuery(Name = "token")]
        public string? Token { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService Swal { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;

        protected override void OnInitialized()
        {
            resetPasswordDTO.PhoneNumber = Phone ?? string.Empty;
            resetPasswordDTO.Token = Token ?? string.Empty;

            if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Token))
            {
                _ = InvalidLinkAsync();
            }
        }

        private async Task InvalidLinkAsync()
        {
            await Swal.FireAsync("Enlace inválido", "El enlace de recuperación no es válido o ha expirado. Solicita uno nuevo.", SweetAlertIcon.Error);
            NavigationManager.NavigateTo("/RecoverPassword");
        }

        private async Task ResetAsync()
        {
            if (resetPasswordDTO.NewPassword != confirmPassword)
            {
                await Swal.FireAsync("Error", "Las contraseñas no coinciden.", SweetAlertIcon.Error);
                return;
            }

            var response = await Repository.PostAsync<ResetPasswordDTO, ActionResponse<string>>("api/Accounts/ResetPassword", resetPasswordDTO);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await Swal.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await Swal.FireAsync("Contraseña restablecida", response.Response!.Message ?? "Inicia session con tu nueva contraseña.", SweetAlertIcon.Success);
            NavigationManager.NavigateTo("/login");
        }
    }
}