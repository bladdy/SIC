using Microsoft.AspNetCore.Components;
using SIC.Frontend.Services;

namespace SIC.Frontend.Pages.UserCredits
{
    public partial class BuyProduct
    {
        [Inject] private PaymentService PaymentService { get; set; } = default!;

        private void goToPurchasingPolicy()
        {
            NavigationManager.NavigateTo("/purchasing-policy");
        }

        private void goToTermsConditions()
        {
            NavigationManager.NavigateTo("/terms-conditions");
        }

        private async Task Pay()
        {
            string priceId = "price_1TeJBMEZzjwN5rDusFUp9OTO"; // Reemplaza con el ID de precio correcto
            var url =
            await PaymentService.CreatePayment(priceId);

            if (!string.IsNullOrEmpty(url))
                NavigationManager.NavigateTo(url, true);
        }
    }
}