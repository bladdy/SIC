using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.Net.Http.Json;

namespace SIC.Frontend.Pages.Users
{
    public partial class UserCreate
    {
        private UserDTO user = new();
        private string password = string.Empty;
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            user = new UserDTO
            {
                UserType = UserType.User,
            };
        }

        private async Task HandleValidSubmit()
        {
            user.UserName = user.PhoneNumber;
            var responseHttp = await repository.PostAsync("api/Accounts/CreateUser", user);

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el usuario.";
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
                 "El usuario se guardo con éxito.",
                SweetAlertIcon.Success
            );
            NavigateToBack();
        }

        private void NavigateToBack() => NavigationManager.NavigateTo("/users");
    }
}