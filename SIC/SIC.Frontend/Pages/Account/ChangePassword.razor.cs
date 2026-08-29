using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Pages.Account
{
    [Authorize]
    public partial class ChangePassword
    {
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private ChangePasswordDTO ChangePasswordModel = new();
        private string ConfirmPassword = string.Empty;

        private bool IsSubmitting = false;

        private bool ShowCurrentPassword = false;
        private bool ShowNewPassword = false;
        private bool ShowConfirmPassword = false;

        private string CurrentPasswordInputType => ShowCurrentPassword ? "text" : "password";
        private string NewPasswordInputType => ShowNewPassword ? "text" : "password";
        private string ConfirmPasswordInputType => ShowConfirmPassword ? "text" : "password";

        private void ToggleCurrentPassword() => ShowCurrentPassword = !ShowCurrentPassword;
        private void ToggleNewPassword() => ShowNewPassword = !ShowNewPassword;
        private void ToggleConfirmPassword() => ShowConfirmPassword = !ShowConfirmPassword;

        private bool CanChangePassword =>
            !string.IsNullOrWhiteSpace(ChangePasswordModel.CurrentPassword) &&
            !string.IsNullOrWhiteSpace(ChangePasswordModel.NewPassword) &&
            !string.IsNullOrWhiteSpace(ConfirmPassword) &&
            ChangePasswordModel.NewPassword.Length >= 6 &&
            ChangePasswordModel.NewPassword == ConfirmPassword;

        private bool ConfirmPasswordMismatch =>
            !string.IsNullOrEmpty(ConfirmPassword) &&
            !string.IsNullOrEmpty(ChangePasswordModel.NewPassword) &&
            ChangePasswordModel.NewPassword != ConfirmPassword;

        private bool NewPasswordTooShort =>
            !string.IsNullOrEmpty(ChangePasswordModel.NewPassword) &&
            ChangePasswordModel.NewPassword.Length < 6;

        private string ConfirmPasswordInputClass => ConfirmPasswordMismatch ? "form-control form-control-lg rounded-start-3 is-invalid" : "form-control form-control-lg rounded-start-3";

        private async Task ChangePasswordAsync()
        {
            if (IsSubmitting)
            {
                return;
            }

            if (!CanChangePassword)
            {
                return;
            }

            IsSubmitting = true;
            try
            {
                var response = await Repository.PostAsync("api/Accounts/ChangePassword", ChangePasswordModel);
                if (response.Error)
                {
                    var message = await response.GetErrorMessageAsync() ?? "No se pudo cambiar la contraseña.";
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return;
                }

                await SweetAlertService.FireAsync(
                    "¡Éxito!",
                    "Tu contraseña ha sido cambiada correctamente.",
                    SweetAlertIcon.Success);

                ChangePasswordModel = new ChangePasswordDTO();
                ConfirmPassword = string.Empty;
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}
