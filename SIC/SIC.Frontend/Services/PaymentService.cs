using CurrieTechnologies.Razor.SweetAlert2;
using SIC.Frontend.Repositories;
using System.Net.Http;
using System.Net.Http.Json;

namespace SIC.Frontend.Services;

public class PaymentService
{
    private readonly IRepository _repository;
    private SweetAlertService sweetAlertService { get; set; } = default!;

    public PaymentService(IRepository repository, SweetAlertService sweetAlertService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        this.sweetAlertService = sweetAlertService ?? throw new ArgumentNullException(nameof(sweetAlertService));
    }

    public async Task<string> CreatePayment(int credits)
    {
        var newPayment = new CreatePaymentRequest { Credits = credits }; // Updated to use a strongly-typed object
        var url = $"api/payments/create-checkout-session?credits={credits}";
        var response = await _repository.PostAsync<CreatePaymentRequest, CreatePaymentResponse>(
            url,
            newPayment); // Updated to match the correct generic signature
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync() ?? "No se pudo crear el pago.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return string.Empty;
        }
        return response.Response?.Url ?? string.Empty;
    }

    public async Task<string> CreatePayment(string priceId)
    {
        HttpResponseWrapper<object>? responseHttp;
        if (string.IsNullOrEmpty(priceId))
        {
            await sweetAlertService.FireAsync("Error", "El ID del precio no puede estar vacío.", SweetAlertIcon.Error);
            return string.Empty;
        }

        responseHttp = await _repository.PostAsync<object, object>(
            "api/payments/pay", priceId
        );

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo generar el pago.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return string.Empty;
        }

        // Ensure the response is not null and cast it to string
        var url = responseHttp.Response as string;

        if (string.IsNullOrEmpty(url))
        {
            await sweetAlertService.FireAsync("Error", "La URL de respuesta es nula o vacía.", SweetAlertIcon.Error);
            return string.Empty;
        }

        return url;
    }
}

public class CreatePaymentRequest // Added a new class to represent the request payload
{
    public int Credits { get; set; }
}

public class CreatePaymentResponse
{
    public string Url { get; set; } = string.Empty;
}