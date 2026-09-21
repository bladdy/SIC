using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Text.Json;

namespace SIC.Frontend.Pages.FormEvents;

[Authorize(Roles = "Admin,WeddingPlanner,User")]
public partial class FormEventResponses
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter] public string EventCode { get; set; } = "";

    private bool Loading = true;
    private bool GeneratingPdf = false;
    private string EventName = "";
    private List<EventFormResponseDTO>? Responses;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Load();
    }

    private async Task Load()
    {
        Loading = true;

        var eventResp = await repository.GetAsync<Event>($"api/Events/byCode/{EventCode}");
        if (eventResp.Error || eventResp.Response == null)
        {
            await sweetAlertService.FireAsync("Error", "No se encontró el evento.", SweetAlertIcon.Error);
            Loading = false;
            return;
        }

        EventName = eventResp.Response.Name;

        var responsesResp = await repository.GetAsync<List<EventFormResponseDTO>>($"api/EventForms/responses/{EventCode}");
        if (responsesResp.Error)
        {
            var message = await responsesResp.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message ?? "No se pudieron cargar las respuestas.", SweetAlertIcon.Error);
            Loading = false;
            return;
        }

        Responses = responsesResp.Response ?? new List<EventFormResponseDTO>();
        Loading = false;
    }

    private async Task Volver()
    {
        await JsRuntime.InvokeVoidAsync("history.back");
    }

    private async Task GeneratePdfAsync()
    {
        if (Responses == null || Responses.Count == 0)
        {
            await sweetAlertService.FireAsync("Error", "No hay respuestas para generar el PDF.", SweetAlertIcon.Error);
            return;
        }

        GeneratingPdf = true;
        try
        {
            var evento = Uri.EscapeDataString(string.IsNullOrWhiteSpace(EventName) ? EventCode : EventName);
            var content = await repository.GetFileAsync($"api/EventForms/responses/{EventCode}/pdf?evento={evento}");

            if (content == null || content.Length == 0)
            {
                await sweetAlertService.FireAsync("Error", "No se pudo generar el PDF.", SweetAlertIcon.Error);
                return;
            }

            await JsRuntime.DownloadFileAsync($"respuestas-{EventCode}.pdf", content, "application/pdf");
        }
        finally
        {
            GeneratingPdf = false;
        }
    }

    private string FormatearValor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.TrimStart().StartsWith("["))
            return value ?? "";

        try
        {
            var lista = JsonSerializer.Deserialize<List<string>>(value);
            return lista is null || lista.Count == 0 ? "" : string.Join(", ", lista);
        }
        catch
        {
            return value;
        }
    }

    private async Task Resetear(int responseId, string? invitationName)
    {
        var name = string.IsNullOrWhiteSpace(invitationName) ? "este invitado" : invitationName;

        var confirm = await sweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = "Resetear respuesta",
            Text = $"¿Borrar la respuesta de {name}? El invitado podrá responder el cuestionario de nuevo.",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Sí, resetear",
            CancelButtonText = "Cancelar",
            ConfirmButtonColor = "#dc3545"
        });

        if (confirm.IsDismissed)
            return;

        var response = await repository.DeleteAsync<bool>($"api/EventForms/responses/{responseId}");
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message ?? "No se pudo resetear la respuesta.", SweetAlertIcon.Error);
            return;
        }

        await Load();

        await sweetAlertService.Mixin(new SweetAlertOptions
        {
            Toast = true,
            Position = SweetAlertPosition.TopEnd,
            ShowConfirmButton = false,
            Timer = 3000,
            TimerProgressBar = true
        }).FireAsync("Listo", "Respuesta eliminada. El invitado ya puede responder de nuevo.", SweetAlertIcon.Success);
    }
}