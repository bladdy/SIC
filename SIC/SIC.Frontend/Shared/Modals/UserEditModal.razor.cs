using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Frontend.Shared.Modals
{
    public partial class UserEditModal
    {
        [Parameter] public bool Show { get; set; }
        [Parameter] public EventCallback<bool> ShowChanged { get; set; }
        [Parameter] public User? UserModel { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }
        [Inject] private IRepository Repository { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;

        private User? user;

        protected override void OnParametersSet()
        {
            if (UserModel != null)
            {
                user = new User
                {
                    Id = UserModel.Id,
                    FirstName = UserModel.FirstName,
                    LastName = UserModel.LastName,
                    Email = UserModel.Email,
                    PhoneNumber = UserModel.PhoneNumber,
                    UserName = UserModel.UserName,
                    Document = UserModel.Document,
                    Address = UserModel.Address,
                    UserType = UserModel.UserType,

                };
            }
        }

        private async Task HandleValidSubmit()
        {
            if (user == null) return;

            var response = await Repository.PutAsync("api/Accounts/UpdateUser", user);

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "No se pudo actualizar el usuario.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await SweetAlertService.FireAsync("Éxito", "Usuario actualizado correctamente.", SweetAlertIcon.Success);
            await OnSaved.InvokeAsync();
            await CloseModal();
        }

        private async Task CloseModal()
        {
            Show = false;
            await ShowChanged.InvokeAsync(false);
        }
    }
}