using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.Entities;
using SIC.Shared.Response;
using System.Security.Claims;

namespace SIC.Frontend.Pages.UserCredits
{
    public partial class BuyProduct
    {
        private string? _userId;
        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] private PaymentService PaymentService { get; set; } = default!;
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

        private List<Product>? StrpeProducts;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity is not null && user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _userId = userId;
            }
            await LoadStrpeProducts();
        }

        private void goToPurchasingPolicy()
        {
            NavigationManager.NavigateTo("/purchasing-policy");
        }

        private void goToTermsConditions()
        {
            NavigationManager.NavigateTo("/terms-conditions");
        }

        private async Task LoadStrpeProducts()
        {
            var response = await repository.GetAsync<List<Product>>($"api/Products");
            if (!response.Error && response.Response is not null)
            {
                StrpeProducts = response.Response;
            }
        }

        /*
        private async Task Pay( string priceId)
        {
            //string priceId = "price_1TeJBMEZzjwN5rDusFUp9OTO"; // Reemplaza con el ID de precio correcto
            var url =
            await PaymentService.CreatePayment(priceId);

            if (!string.IsNullOrEmpty(url))
                NavigationManager.NavigateTo(url, true);
        }*/
        /*
        private async Task Pay(string priceId)
        {
            var url = await PaymentService.CreatePayment(priceId);

            if (!string.IsNullOrEmpty(url))
            {
                await JsRuntime.InvokeVoidAsync(
                    "open",
                    url,
                    "StripeCheckout",
                    "width=1200,height=800,left=100,top=100,resizable=yes,scrollbars=yes"
                );
            }
        }*/
        private async Task Pay(int productid)
        {
            if (_userId == null) return;
            var url = await PaymentService.CreatePayment(productid, _userId);

            if (!string.IsNullOrEmpty(url))
            {
                await JsRuntime.InvokeVoidAsync(
                    "location.assign",
                    url
                );
            }
        }
    }
}