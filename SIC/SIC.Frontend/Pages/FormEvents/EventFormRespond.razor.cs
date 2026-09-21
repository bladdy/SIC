using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Pages.FormEvents;

public partial class EventFormRespond
{
    [Inject] private IRepository repository { get; set; } = default!;
    [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

    [Parameter] public string InvitationCode { get; set; } = "";

    private bool Loading = true;
    private bool Sending = false;
    private bool CanRespond = false;
    private string BlockedMessage = "No se encontró la invitación.";
    private string EventName = "";
    private string EventTitle = "";
    private FormularioModel Formulario = new();
    private List<InvitationGuest> AttendGuests = new();
    private Dictionary<int, Dictionary<Guid, string>> RespuestasPorGuest = new();
    private Dictionary<int, HashSet<Guid>> FailedPorGuest = new();
    private HashSet<int> Respondidos = new();

    private const int LegacyGuestKey = 0;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await Load();
    }

    private async Task Load()
    {
        Loading = true;

        var invitationResp = await repository.GetAsync<Invitation>($"api/Invitations/byCode/{InvitationCode}");
        if (invitationResp.Error || invitationResp.Response == null)
        {
            BlockedMessage = "No se encontró la invitación.";
            Loading = false;
            return;
        }

        var invitation = invitationResp.Response;

        if (invitation.Status != Status.Attend)
        {
            BlockedMessage = "Aún no has confirmado tu asistencia.";
            Loading = false;
            return;
        }

        if (invitation.Event == null || string.IsNullOrWhiteSpace(invitation.Event.Code))
        {
            BlockedMessage = "No se pudo cargar la información del evento.";
            Loading = false;
            return;
        }

        EventName = invitation.Event.Name ?? string.Empty;
        EventTitle = invitation.Event.SubTitle ?? string.Empty;
        var formResp = await repository.GetAsync<EventFormDTO>($"api/EventForms/byEventCode/{invitation.Event.Code}");
        if (formResp.Error || formResp.Response?.Formulario == null || formResp.Response.Formulario.Preguntas.Count == 0)
        {
            BlockedMessage = "El evento aún no tiene un cuestionario configurado.";
            Loading = false;
            return;
        }

        Formulario = formResp.Response.Formulario;
        if (!string.IsNullOrWhiteSpace(formResp.Response.Title))
            Formulario.Titulo = formResp.Response.Title;

        AttendGuests = invitation.Guests?.Where(g => g.Status == Status.Attend).ToList() ?? new List<InvitationGuest>();

        RespuestasPorGuest = new Dictionary<int, Dictionary<Guid, string>>();
        FailedPorGuest = new Dictionary<int, HashSet<Guid>>();
        Respondidos = new HashSet<int>();

        foreach (var guest in AttendGuests)
        {
            RespuestasPorGuest[guest.Id] = new Dictionary<Guid, string>();
            FailedPorGuest[guest.Id] = new HashSet<Guid>();
        }

        // Invitaciones sin invitados registrados usan la clave LegacyGuestKey
        if (AttendGuests.Count == 0)
        {
            RespuestasPorGuest[LegacyGuestKey] = new Dictionary<Guid, string>();
            FailedPorGuest[LegacyGuestKey] = new HashSet<Guid>();
        }

        var existingResp = await repository.GetAsync<List<EventFormResponseDTO>>($"api/EventForms/byInvitation/{InvitationCode}");
        if (!existingResp.Error && existingResp.Response != null)
        {
            foreach (var response in existingResp.Response)
            {
                var key = response.InvitationGuestId ?? LegacyGuestKey;
                Respondidos.Add(key);
                if (!RespuestasPorGuest.ContainsKey(key))
                    RespuestasPorGuest[key] = new Dictionary<Guid, string>();
                if (!FailedPorGuest.ContainsKey(key))
                    FailedPorGuest[key] = new HashSet<Guid>();

                if (response.Answers != null)
                {
                    foreach (var answer in response.Answers)
                        RespuestasPorGuest[key][answer.QuestionId] = answer.Value ?? string.Empty;
                }
            }
        }

        CanRespond = true;
        Loading = false;
    }

    private async Task Enviar()
    {
        var failed = Validate();
        if (failed.Count > 0)
        {
            await sweetAlertService.FireAsync("Validación",
                "Por favor completa las preguntas obligatorias marcadas en rojo.",
                SweetAlertIcon.Warning);
            return;
        }

        var guestAnswers = BuildGuestAnswers();

        Sending = true;
        var dto = new SubmitEventFormDTO
        {
            InvitationCode = InvitationCode,
            GuestAnswers = guestAnswers
        };

        var response = await repository.PostAsync("api/EventForms/respond", dto);
        if (response.Error)
        {
            var message = await response.GetErrorMessageAsync();
            await sweetAlertService.FireAsync("Error", message ?? "No se pudo enviar el cuestionario.", SweetAlertIcon.Error);
            Sending = false;
            return;
        }

        Sending = false;

        await sweetAlertService.FireAsync(
            "¡Gracias!",
            "Tus respuestas fueron enviadas correctamente.",
            SweetAlertIcon.Success);

        await Load();
    }

    private Dictionary<int, HashSet<Guid>> Validate()
    {
        var failed = new Dictionary<int, HashSet<Guid>>();
        foreach (var guest in AttendGuests)
            ValidateGuest(guest.Id, failed);
        if (AttendGuests.Count == 0)
            ValidateGuest(LegacyGuestKey, failed);
        return failed;
    }

    private void ValidateGuest(int key, Dictionary<int, HashSet<Guid>> failed)
    {
        if (Respondidos.Contains(key))
            return;

        var guestFailed = new HashSet<Guid>();
        var respuestas = RespuestasPorGuest.GetValueOrDefault(key) ?? new Dictionary<Guid, string>();
        foreach (var pregunta in Formulario.Preguntas)
            ValidateQuestion(pregunta, respuestas, guestFailed);

        if (guestFailed.Count > 0)
            failed[key] = guestFailed;
    }

    private void ValidateQuestion(PreguntaModel pregunta, Dictionary<Guid, string> respuestas, HashSet<Guid> failed)
    {
        var value = respuestas.GetValueOrDefault(pregunta.Id);

        if (pregunta.Obligatoria && string.IsNullOrWhiteSpace(value))
            failed.Add(pregunta.Id);

        if (pregunta.Tipo == TipoPregunta.SiNo)
        {
            foreach (var regla in pregunta.Reglas.Where(r => r.ValorDisparador == value))
            {
                foreach (var sub in regla.SubPreguntas)
                    ValidateQuestion(sub, respuestas, failed);
            }
        }
    }

    private List<GuestAnswersDTO> BuildGuestAnswers()
    {
        var result = new List<GuestAnswersDTO>();

        foreach (var guest in AttendGuests)
        {
            if (Respondidos.Contains(guest.Id))
                continue;
            result.Add(new GuestAnswersDTO
            {
                InvitationGuestId = guest.Id,
                Answers = CollectAnswers(guest.Id)
            });
        }

        if (AttendGuests.Count == 0 && !Respondidos.Contains(LegacyGuestKey))
        {
            result.Add(new GuestAnswersDTO
            {
                InvitationGuestId = null,
                Answers = CollectAnswers(LegacyGuestKey)
            });
        }

        return result;
    }

    private List<FormAnswerDTO> CollectAnswers(int key)
    {
        var result = new List<FormAnswerDTO>();
        var respuestas = RespuestasPorGuest.GetValueOrDefault(key) ?? new Dictionary<Guid, string>();
        foreach (var pregunta in Formulario.Preguntas)
            CollectQuestion(pregunta, respuestas, result);
        return result;
    }

    private void CollectQuestion(PreguntaModel pregunta, Dictionary<Guid, string> respuestas, List<FormAnswerDTO> result)
    {
        var value = respuestas.GetValueOrDefault(pregunta.Id);

        result.Add(new FormAnswerDTO
        {
            QuestionId = pregunta.Id,
            QuestionTitle = pregunta.Titulo,
            Value = value
        });

        if (pregunta.Tipo == TipoPregunta.SiNo)
        {
            foreach (var regla in pregunta.Reglas.Where(r => r.ValorDisparador == value))
            {
                foreach (var sub in regla.SubPreguntas)
                    CollectQuestion(sub, respuestas, result);
            }
        }
    }

    private bool GuestResponded(int key) => Respondidos.Contains(key);

    private bool QuedanPendientes
    {
        get
        {
            if (AttendGuests.Count == 0)
                return !Respondidos.Contains(LegacyGuestKey);
            return AttendGuests.Any(g => !Respondidos.Contains(g.Id));
        }
    }
}