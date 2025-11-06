using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Shared.Modals
{
    public partial class UserCreateModal
    {
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        [Parameter] public bool Show { get; set; }
        [Parameter] public EventCallback<bool> ShowChanged { get; set; }

        [Parameter] public User? UserModel { get; set; }
        [Parameter] public EventCallback OnSaved { get; set; }

        private UserDTO user = new();
        private bool IsEditMode => string.IsNullOrWhiteSpace(user.Id);
        private string ModalTitle => IsEditMode ? "Editar Usuario" : "Crear Usuario";

        protected override void OnParametersSet()
        {
            user = UserModel != null ? new UserDTO
            {
                Id = UserModel.Id,
                FirstName = UserModel.FirstName,
                LastName = UserModel.LastName,
                Email = UserModel.Email,
                PhoneNumber = UserModel.PhoneNumber,
                Document = UserModel.Document,
                Address = UserModel.Address,
                UserType = UserModel.UserType,
                Password = string.Empty,
                PasswordConfirm = string.Empty
            } : new UserDTO { UserType = UserType.User };
        }

        private async Task HandleValidSubmit()
        {
            user.UserName = user.PhoneNumber;
            user.Email = user.PhoneNumber + "@sic.com";
            user.Document = user.PhoneNumber;
            user.Address = user.PhoneNumber;
            var endpoint = IsEditMode ? "api/Accounts/UpdateUser" : "api/Accounts/CreateUser";
            var response = IsEditMode
                ? await Repository.PutAsync(endpoint, user)
                : await Repository.PostAsync(endpoint, user);

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "No se pudo guardar el usuario.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await SweetAlertService.FireAsync("Éxito", $"Usuario {(IsEditMode ? "actualizado" : "creado")} correctamente.", SweetAlertIcon.Success);

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