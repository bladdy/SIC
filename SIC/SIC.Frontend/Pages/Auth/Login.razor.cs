using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Pages.Auth
{
    public partial class Login
    {
        private LoginDTO loginDTO { get; set; } = new();
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService Swal { get; set; } = null!;
        [Inject] private ILoginService LoginService { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;

        private async Task LoginAsync()
        {
            var response = await Repository.PostAsync<LoginDTO, TokenDTO>("api/Accounts/Login", loginDTO);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await Swal.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            await LoginService.LoginAsync(response.Response!.Token);
            NavigationManager.NavigateTo("/");
        }
    }
}