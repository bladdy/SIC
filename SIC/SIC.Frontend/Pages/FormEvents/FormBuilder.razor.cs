using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Pages.FormEvents;

[Authorize(Roles = "Admin,WeddingPlanner,User")]
public partial class FormBuilder
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter] public string EventCode { get; set; } = "";

    private bool Loading = true;
    private bool Saving = false;
    private int EventId;
    private string EventName = "";
    private FormularioModel Formulario = new();

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

        EventId = eventResp.Response.Id;
        EventName = eventResp.Response.Name;

        var formResp = await repository.GetAsync<EventFormDTO>($"api/EventForms/byEventCode/{EventCode}");
        if (formResp.Error)
        {
            var message = await formResp.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message ?? "No se pudo cargar el formulario del evento.", SweetAlertIcon.Error);
            Loading = false;
            return;
        }

        if (formResp.Response?.Formulario != null)
        {
            Formulario = formResp.Response.Formulario;
        }

        Formulario.Titulo = EventName;

        if (Formulario.Preguntas.Count > 0)
            EnsureQuestionIds(Formulario);

        Loading = false;
    }

    private void EnsureQuestionIds(FormularioModel formulario)
    {
        foreach (var pregunta in formulario.Preguntas)
        {
            if (pregunta.Id == Guid.Empty)
                pregunta.Id = Guid.NewGuid();

            foreach (var regla in pregunta.Reglas)
            {
                foreach (var sub in regla.SubPreguntas)
                {
                    if (sub.Id == Guid.Empty)
                        sub.Id = Guid.NewGuid();
                }
            }
        }
    }

    private void Agregar()
    {
        Formulario.Preguntas.Add(new PreguntaModel
        {
            Id = Guid.NewGuid(),
            Titulo = "Nueva pregunta",
            Tipo = TipoPregunta.Texto
        });
    }

    private void Eliminar(PreguntaModel pregunta)
    {
        Formulario.Preguntas.Remove(pregunta);
    }

    private async Task Volver()
    {
        await JsRuntime.InvokeVoidAsync("history.back");
    }

    private async Task Guardar()
    {
        if (Formulario.Preguntas.Count == 0)
        {
            await sweetAlertService.FireAsync("Validación",
                "El formulario debe contener al menos una pregunta.",
                SweetAlertIcon.Warning);
            return;
        }

        if (Formulario.Preguntas.Any(p => string.IsNullOrWhiteSpace(p.Titulo)))
        {
            await sweetAlertService.FireAsync("Validación",
                "Todas las preguntas deben tener un título.",
                SweetAlertIcon.Warning);
            return;
        }

        Formulario.Titulo = EventName;

        Saving = true;
        var dto = new SaveEventFormDTO
        {
            EventId = EventId,
            Formulario = Formulario
        };

        var response = await repository.PostAsync("api/EventForms/save", dto);
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message ?? "No se pudo guardar el formulario.", SweetAlertIcon.Error);
            Saving = false;
            return;
        }

        Saving = false;
        await sweetAlertService.Mixin(new SweetAlertOptions
        {
            Toast = true,
            Position = SweetAlertPosition.TopEnd,
            ShowConfirmButton = false,
            Timer = 3000,
            TimerProgressBar = true
        }).FireAsync("Guardado", "Formulario guardado correctamente.", SweetAlertIcon.Success);
    }
}