using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;

namespace SIC.Frontend.Pages.Users
{
    public partial class UserEdit
    {
        private string? userRol = string.Empty;
        [Parameter] public string Id { get; set; } = string.Empty;
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        private User? user;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var userstate = authState.User;
            if (userstate.Identity is not null && userstate.Identity.IsAuthenticated)
            {
                userRol = userstate.FindFirst(ClaimTypes.Role)?.Value;
            }
            var responseHttp = await repository.GetAsync<User>($"api/accounts/UserById/{Id}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                }
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);

                NavigationManager.NavigateTo("/users");
            }
            user = responseHttp.Response!;
        }

        private async Task HandleValidSubmit()
        {
            var responseHttp = await repository.PutAsync
                ("api/Accounts/UpdateUser", user);

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo actualizar el usuario.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            // Luego mostrar la notificación
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync(
                "Éxito",
                 "El usuario actualizado con éxito.",
                SweetAlertIcon.Success
            );

            NavigateToBack();
        }

        private void NavigateToBack()
        {
            if (userRol == UserType.Admin.ToString())
                NavigationManager.NavigateTo("/users");
            else
                NavigationManager.NavigateTo("/");
        }
    }
}