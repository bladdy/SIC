using CurrieTechnologies.Razor.SweetAlert2;
using SIC.Frontend.Repositories;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

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
        HttpResponseWrapper<CreatePaymentResponse>? responseHttp;

        if (string.IsNullOrEmpty(priceId))
        {
            await sweetAlertService.FireAsync("Error", "El ID del precio no puede estar vacío.", SweetAlertIcon.Error);
            return string.Empty;
        }

        responseHttp = await _repository.PostAsync<bool, CreatePaymentResponse>(
           $"api/payments/pay/{priceId}", true
       );

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo generar el pago.";
            await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return string.Empty;
        }

        var url = responseHttp.Response?.Url;

        if (string.IsNullOrWhiteSpace(url))
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