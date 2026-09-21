using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SIC.Backend.Repositories.Implemetations;

public class EventFormsRepository : GenericRepository<EventForm>, IEventFormsRepository
{
    private readonly DataContext _context;

    public EventFormsRepository(DataContext context) : base(context)
    {
        _context = context;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<ActionResponse<EventFormDTO>> GetByEventCodeAsync(string eventCode)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Code == eventCode);
        if (ev == null)
        {
            return new ActionResponse<EventFormDTO>
            {
                Message = "No se encontró el evento."
            };
        }

        var form = await _context.EventForms.FirstOrDefaultAsync(f => f.EventId == ev.Id);

        var dto = new EventFormDTO
        {
            EventId = ev.Id,
            EventCode = ev.Code,
            EventName = ev.Name,
            Title = form?.Title,
            Formulario = form?.FormJson == null
                ? new FormularioModel()
                : DeserializeForm(form.FormJson)
        };

        return new ActionResponse<EventFormDTO>
        {
            Success = true,
            Result = dto
        };
    }

    public async Task<ActionResponse<bool>> SaveAsync(int eventId, SaveEventFormDTO dto)
    {
        if (await _context.Events.AnyAsync(e => e.Id == eventId) == false)
        {
            return new ActionResponse<bool>
            {
                Message = "El evento no existe."
            };
        }

        var formJson = JsonSerializer.Serialize(dto.Formulario, JsonOptions);

        var form = await _context.EventForms.FirstOrDefaultAsync(f => f.EventId == eventId);
        if (form == null)
        {
            form = new EventForm
            {
                EventId = eventId,
                Title = dto.Formulario.Titulo,
                FormJson = formJson,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.EventForms.Add(form);
        }
        else
        {
            form.Title = dto.Formulario.Titulo;
            form.FormJson = formJson;
            form.UpdatedAt = DateTime.UtcNow;
            _context.EventForms.Update(form);
        }

        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> GetResponsesByEventAsync(string eventCode)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Code == eventCode);
        if (ev == null)
        {
            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Message = "No se encontró el evento."
            };
        }

        var responses = await _context.EventFormResponses
            .Where(r => r.EventId == ev.Id)
            .OrderBy(r => r.InvitationId)
            .ThenBy(r => r.GuestName)
            .ToListAsync();

        var result = responses.Select(r => new EventFormResponseDTO
        {
            Id = r.Id,
            EventId = r.EventId,
            InvitationId = r.InvitationId,
            InvitationGuestId = r.InvitationGuestId,
            InvitationName = r.InvitationName,
            GuestName = r.GuestName,
            Answers = DeserializeAnswers(r.AnswersJson),
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        });

        return new ActionResponse<IEnumerable<EventFormResponseDTO>>
        {
            Success = true,
            Result = result
        };
    }

    public async Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> GetResponseByInvitationAsync(string invitationCode)
    {
        var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.Code == invitationCode);
        if (invitation == null)
        {
            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Success = true,
                Result = new List<EventFormResponseDTO>()
            };
        }

        var payloads = await _context.EventFormResponses
            .Where(r => r.InvitationId == invitation.Id)
            .OrderBy(r => r.InvitationGuestId)
            .ToListAsync();

        var result = payloads.Select(p => new EventFormResponseDTO
        {
            Id = p.Id,
            EventId = p.EventId,
            InvitationId = p.InvitationId,
            InvitationGuestId = p.InvitationGuestId,
            InvitationName = p.InvitationName,
            GuestName = p.GuestName,
            Answers = DeserializeAnswers(p.AnswersJson),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });

        return new ActionResponse<IEnumerable<EventFormResponseDTO>>
        {
            Success = true,
            Result = result
        };
    }

    public async Task<ActionResponse<IEnumerable<EventFormResponseDTO>>> SaveResponseAsync(SubmitEventFormDTO dto)
    {
        var invitation = await _context.Invitations
            .Include(i => i.Guests)
            .FirstOrDefaultAsync(i => i.Code == dto.InvitationCode);

        if (invitation == null)
        {
            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Message = "No se encontró la invitación."
            };
        }

        if (invitation.Status != Status.Attend)
        {
            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Message = "Solo podrás responder el cuestionario si confirmaste que asistirás al evento."
            };
        }

        var hasForm = await _context.EventForms.AnyAsync(f => f.EventId == invitation.EventId);
        if (!hasForm)
        {
            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Message = "El evento aún no tiene un cuestionario configurado."
            };
        }

        if (dto.GuestAnswers == null || dto.GuestAnswers.Count == 0)
        {
            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Message = "No se recibieron respuestas del cuestionario."
            };
        }

        var toCreate = new List<EventFormResponse>();
        var created = new List<EventFormResponseDTO>();

        foreach (var guestAnswer in dto.GuestAnswers)
        {
            // Respuesta a nivel de invitación (invitaciones sin invitados registrados)
            if (guestAnswer.InvitationGuestId is null || guestAnswer.InvitationGuestId == 0)
            {
                var exists = await _context.EventFormResponses.AnyAsync(r =>
                    r.EventId == invitation.EventId &&
                    r.InvitationId == invitation.Id &&
                    r.InvitationGuestId == null);

                if (exists)
                {
                    return new ActionResponse<IEnumerable<EventFormResponseDTO>>
                    {
                        Message = "Ya enviaste tus respuestas. No puedes enviarlas de nuevo."
                    };
                }

                toCreate.Add(new EventFormResponse
                {
                    EventId = invitation.EventId,
                    InvitationId = invitation.Id,
                    InvitationGuestId = null,
                    InvitationName = invitation.Name,
                    GuestName = null,
                    AnswersJson = JsonSerializer.Serialize(guestAnswer.Answers ?? new List<FormAnswerDTO>(), JsonOptions),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                continue;
            }

            var guest = invitation.Guests.FirstOrDefault(g => g.Id == guestAnswer.InvitationGuestId);
            if (guest == null)
            {
                return new ActionResponse<IEnumerable<EventFormResponseDTO>>
                {
                    Message = "Uno de los invitados no pertenece a esta invitación."
                };
            }

            if (guest.Status != Status.Attend)
            {
                return new ActionResponse<IEnumerable<EventFormResponseDTO>>
                {
                    Message = $"El invitado {guest.GuestName} debe confirmar su asistencia antes de responder el cuestionario."
                };
            }

            var guestExists = await _context.EventFormResponses.AnyAsync(r =>
                r.EventId == invitation.EventId &&
                r.InvitationGuestId == guest.Id);

            if (guestExists)
            {
                return new ActionResponse<IEnumerable<EventFormResponseDTO>>
                {
                    Message = $"El invitado {guest.GuestName} ya envió sus respuestas. No puede enviarlas de nuevo."
                };
            }

            toCreate.Add(new EventFormResponse
            {
                EventId = invitation.EventId,
                InvitationId = invitation.Id,
                InvitationGuestId = guest.Id,
                InvitationName = invitation.Name,
                GuestName = guest.GuestName,
                AnswersJson = JsonSerializer.Serialize(guestAnswer.Answers ?? new List<FormAnswerDTO>(), JsonOptions),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context.EventFormResponses.AddRange(toCreate);

        try
        {
            await _context.SaveChangesAsync();

            created = toCreate.Select(p => new EventFormResponseDTO
            {
                Id = p.Id,
                EventId = p.EventId,
                InvitationId = p.InvitationId,
                InvitationGuestId = p.InvitationGuestId,
                InvitationName = p.InvitationName,
                GuestName = p.GuestName,
                Answers = DeserializeAnswers(p.AnswersJson),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Success = true,
                Result = created
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<IEnumerable<EventFormResponseDTO>>
            {
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> ResetResponseAsync(int responseId)
    {
        var payload = await _context.EventFormResponses.FirstOrDefaultAsync(r => r.Id == responseId);
        if (payload == null)
        {
            return new ActionResponse<bool>
            {
                Message = "No se encontró la respuesta."
            };
        }

        _context.EventFormResponses.Remove(payload);

        try
        {
            await _context.SaveChangesAsync();
            return new ActionResponse<bool>
            {
                Success = true,
                Result = true
            };
        }
        catch (Exception ex)
        {
            return new ActionResponse<bool>
            {
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResponse<bool>> ExistsAsync(string eventCode)
    {
        var exists = await _context.EventForms
            .Join(_context.Events, f => f.EventId, e => e.Id, (f, e) => new { f, e })
            .AnyAsync(x => x.e.Code == eventCode);

        return new ActionResponse<bool>
        {
            Success = true,
            Result = exists
        };
    }

    private static FormularioModel DeserializeForm(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<FormularioModel>(json, JsonOptions) ?? new FormularioModel();
        }
        catch
        {
            return new FormularioModel();
        }
    }

    private static List<FormAnswerDTO> DeserializeAnswers(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<FormAnswerDTO>();

        try
        {
            return JsonSerializer.Deserialize<List<FormAnswerDTO>>(json, JsonOptions) ?? new List<FormAnswerDTO>();
        }
        catch
        {
            return new List<FormAnswerDTO>();
        }
    }
}