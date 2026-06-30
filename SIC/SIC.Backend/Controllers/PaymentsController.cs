using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using Stripe;
using Stripe.Checkout;
using static System.Net.WebRequestMethods;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : Controller
{
    private readonly ProductService _product;
    private readonly IGenericUnitOfWork<SIC.Shared.Entities.Product> _unitOfWorkProduct;
    private readonly IUserCreditUnitsOfWork _creditUnitsOfWork;
    private readonly IWhatsAppConfigUnitOfWork _whatsAppConfigUnitOfWork;

    public PaymentsController(ProductService product,
            IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork, IUserCreditUnitsOfWork creditUnitsOfWork, IGenericUnitOfWork<SIC.Shared.Entities.Product> unitOfWorkProduct)
    {
        _product = product;
        _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
        _creditUnitsOfWork = creditUnitsOfWork;
        _unitOfWorkProduct = unitOfWorkProduct;
    }

    [HttpPost("pay/{productid}/{userId}")]
    public async Task<IActionResult> Pay(int productid, string userId)
    {
        var products = await _unitOfWorkProduct.GetAsync(productid);
        if (products.Result == null)
            return BadRequest(new { error = "El producto no existe." });

        var stripe = await _whatsAppConfigUnitOfWork.GetStripeConfig("DEV");
        if (stripe.Result == null)
            return BadRequest(new { error = "No se puedo encontrar Stripe." });

        StripeConfiguration.ApiKey = stripe.Result.SecretKey;
        var options = new SessionCreateOptions
        {
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "mxn",
                        UnitAmount = (long)(products.Result.PriceTotal * 100), // Stripe espera centavos

                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = products.Result.Name,
                            Images =
                            [
                                products.Result.URLImagen
                            ],
                            Description = string.Join(", ", products.Result.Items)
                        }
                    }
                }
            ],
            Mode = "payment",

            SuccessUrl = "https://invboxv-app.com/successful-Purchase?session_id={CHECKOUT_SESSION_ID}",

            CancelUrl = "https://invboxv-app.com/users-credits/details/",

            Metadata = new Dictionary<string, string>
            {
                { "UserId", userId },
                { "Credits", products.Result.Amount.ToString() }
            }
        };
        var service = new SessionService();
        Session session = await service.CreateAsync(options);
        return Ok(new
        {
            Url = session.Url
        });
    }

    [HttpGet("GetAllProducts")]
    public async Task<IActionResult> Get()
    {
        var stripe = await _whatsAppConfigUnitOfWork.GetStripeConfig("DEV");
        if (stripe.Result == null)
            return BadRequest(new { error = "No se puedo encontrar Stripe." });

        StripeConfiguration.ApiKey = stripe.Result.SecretKey;
        var options = new ProductListOptions { Expand = new List<string> { "data.default_price" } };
        var products = _product.List(options);
        return Ok(products.Data);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        //const string endpointSecret = "whsec_P2tDvgNJUOkpz4ICIyD5IcXmxVeSifSb";
        const string endpointSecret = "whsec_oSYmdn9KVRVCkOc0qQ7e6W1OQgFKWyu6";
        try
        {
            var signatureHeader = Request.Headers["Stripe-Signature"];

            var stripeEvent = EventUtility.ConstructEvent(
                json,
                signatureHeader,
                endpointSecret,
                throwOnApiVersionMismatch: false);

            if (stripeEvent.Type == EventTypes.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Session;

                if (session != null)
                {
                    var userId = Guid.Parse(session.Metadata["UserId"]);
                    var credits = int.Parse(session.Metadata["Credits"]);

                    var amountPaid = session.AmountTotal.GetValueOrDefault();

                    // Evitar duplicados (recomendado)
                    var exists = await _creditUnitsOfWork.ExistStripeEventLogAsync(stripeEvent.Id);
                    if (exists.Result) return Ok();

                    // Guardar la compra
                    await _creditUnitsOfWork.AddStripeEventLogAsync(new StripeEventLog
                    {
                        EventId = stripeEvent.Id,
                        ProcessedAt = DateTime.UtcNow
                    });

                    var userCredit = new AddCreditsRequest
                    {
                        CreditsToAdd = credits,
                        UserId = userId.ToString(),
                        UpdatedBy = "Compra hecha por Stripe",
                        Notes = $"Usuario hizo la compra de ({credits}) Créditos."
                    };
                    await _creditUnitsOfWork.AddAsync(userCredit);
                }
            }
            else if (stripeEvent.Type == EventTypes.PaymentMethodAttached)
            {
                var paymentMethod = stripeEvent.Data.Object as PaymentMethod;

                Console.WriteLine(
                    $"Payment Method Attached: {paymentMethod?.Id}");
            }
            else
            {
                Console.WriteLine(
                    $"Unhandled event type: {stripeEvent.Type}");
            }

            return Ok();
        }
        catch (StripeException e)
        {
            Console.WriteLine($"Stripe Error: {e.Message}");
            return BadRequest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StatusCode(500);
        }
    }
}