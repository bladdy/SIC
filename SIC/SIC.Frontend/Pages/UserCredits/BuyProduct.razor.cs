using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Frontend.Pages.UserCredits
{
    public partial class BuyProduct
    {
        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] private PaymentService PaymentService { get; set; } = default!;
        [Inject] private IRepository repository { get; set; } = default!;

        private List<StripeProductResponse>? StrpeProducts;

        protected override async Task OnInitializedAsync()
        {
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
            var response = await repository.GetAsync<List<StripeProductResponse>>($"api/payments/GetAllProducts");
            if (!response.Error && response.Response is not null)
            {
                StrpeProducts = response.Response
                    .OrderBy(x => x.DefaultPrice.UnitAmount)
                    .ToList();
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
        }
    }
}