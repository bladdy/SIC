using Microsoft.AspNetCore.Components;
using SIC.Frontend.Services;

namespace SIC.Frontend.Pages.Auth
{
    public partial class Logout
    {
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private ILoginService LoginService { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            await LoginService.LogOutAsync();
            NavigationManager.NavigateTo("/");
        }
    }
}