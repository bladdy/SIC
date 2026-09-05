using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using System.Security.Claims;

namespace SIC.Frontend.Pages.Whatsapp
{
    public partial class WhatsappConfigPage
    {
        private string userID = null!;
        private bool ShowToken = false;

        // false → password (oculto)
        // true  → text (visible)
        private string TokenInputType => ShowToken ? "text" : "password";

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;
        [Inject] private IConfiguration Configuration { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        private WhatsAppManualConfigDto Model = new();
        private bool IsSaving = false;

        //TODO: hacer que sea por el {Model.PhoneNumber} para que mas de un usuario pueda configurar el mismo whatsapp o tenga aceso a la misma bandeja
        //ToDo: agregar que {Model.PhoneNumber} sea requerido y tenga validación de formato de número telefónico internacional
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState != null)
            {
                var user = authState.User;
                if (user.Identity != null && user.Identity.IsAuthenticated)
                {
                    ///api/whatsapp/webhook/{phone} https://nonmanifest-dangly-johnetta.ngrok-free.dev
                    await LoadConfiguraitons();
                    userID = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                    Model.WebhookVerificationToken = "invboxv_wh_7F9KpQ2eA8LxM4RZCwN6DHyT5B"; // Usa el userID como token de verificación del webhook
                    Model.WebhookUrl = $"{NavigationManager.BaseUri}api/whatsapp/webhook"; // Establece la URL del webhook apuntando a tu API

                    //Model.WebhookUrl = $"https://nonmanifest-dangly-johnetta.ngrok-free.dev/api/whatsapp/webhook/{Model.PhoneNumber}";
                }
            }
        }

        private async Task LoadConfiguraitons()
        {
            var response = await Repository.GetAsync<WhatsAppManualConfigDto>(
                "/api/whatsapp/configurar"
            );
            Model = response.Response ?? new();
        }

        private bool IsFormComplete =>
            !string.IsNullOrWhiteSpace(Model.BusinessId) &&
            !string.IsNullOrWhiteSpace(Model.WabaId) &&
            !string.IsNullOrWhiteSpace(Model.PhoneNumberId) &&
            !string.IsNullOrWhiteSpace(Model.PhoneNumber) &&
            !string.IsNullOrWhiteSpace(Model.AccessToken);

        private async Task GuardarConfiguracion()
        {
            IsSaving = true;

            var response = await Repository.PostAsync<object>(
                "/api/whatsapp/configurar",
                Model
            );

            IsSaving = false;

            if (response.Error)
            {
                var msg = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync(
                    "Error",
                    msg,
                    SweetAlertIcon.Error
                );
                return;
            }

            await SweetAlertService.FireAsync(
                "Configuración guardada",
                "WhatsApp quedó configurado correctamente",
                SweetAlertIcon.Success
            );
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var appId = Configuration["Facebook:AppId"];// Obtém o App ID do Facebook de la base de datos
                await JS.InvokeVoidAsync("initFacebookSdk", appId);
            }
        }

        private void ToggleToken()
        {
            ShowToken = !ShowToken;
        }

        private async Task LoginWhatsApp()
        {
            var config_id = Configuration["Facebook:configId3"];// Obtém o config_id de la base de datos
            await JS.InvokeVoidAsync("whatsappEmbeddedSignup", config_id);
        }

        private async Task LogoutWhatsApp()
        {
            await JS.InvokeVoidAsync("logoutFacebook");
        }

        private async Task CopyToClipboard(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            await JS.InvokeVoidAsync("navigator.clipboard.writeText", value);
        }
    }
}