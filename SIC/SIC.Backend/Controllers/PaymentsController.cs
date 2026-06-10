using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SIC.Shared.DTOs;
using SIC.Shared.Request;
using Stripe;
using Stripe.Checkout;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly StripeSettings _stripeSettings;
    private readonly ProductService _product;

    public PaymentsController(IOptions<StripeSettings> model, ProductService product)
    {
        _stripeSettings = model.Value;
        _product = product;
    }

    [HttpPost("pay/{priceId}")]
    public async Task<IActionResult> Pay(string priceId)
    {
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        var options = new SessionCreateOptions
        {
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = "https://localhost:7174/successful-Purchase",
            CancelUrl = "https://localhost:7174/",
        };
        var service = new SessionService();
        Session session = await service.CreateAsync(options);
        return Ok(new
        {
            Url = session.Url
        });
    }

    [HttpGet("GetAllProducts")]
    public IActionResult Get()
    {
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        var options = new ProductListOptions { Expand = new List<string> { "data.default_price" } };
        var products = _product.List(options);
        return Ok(products.Data);
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession(
            BuyCreditsRequest request)
    {
        decimal creditPrice = 150m;

        decimal total =
            request.Credits * creditPrice;

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes =
            [
                "card"
            ],

            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData =
                        new SessionLineItemPriceDataOptions
                        {
                            Currency = "mxn",

                            UnitAmount =
                                (long)(creditPrice * 100),

                            ProductData =
                                new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name =
                                        $"{request.Credits} Créditos"
                                }
                        },

                    Quantity = request.Credits
                }
            ],

            Mode = "payment",

            SuccessUrl =
                "https://tuapp.com/payment-success?session_id={CHECKOUT_SESSION_ID}",

            CancelUrl =
                "https://tuapp.com/payment-cancel",

            Metadata = new Dictionary<string, string>
            {
                ["credits"] = request.Credits.ToString()
            }
        };

        var service = new SessionService();

        var session =
            await service.CreateAsync(options);

        return Ok(new
        {
            session.Url
        });
    }
}