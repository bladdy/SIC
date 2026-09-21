using SIC.Shared.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class EventFormDTO
{
    public int EventId { get; set; }
    public string? EventCode { get; set; }
    public string? EventName { get; set; }
    public string? Title { get; set; }
    public FormularioModel? Formulario { get; set; }
}

public class SaveEventFormDTO
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int EventId { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public FormularioModel Formulario { get; set; } = new();
}

public class FormAnswerDTO
{
    public Guid QuestionId { get; set; }

    [Display(Name = "Pregunta")]
    public string? QuestionTitle { get; set; }

    [Display(Name = "Respuesta")]
    public string? Value { get; set; }
}

public class EventFormResponseDTO
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int InvitationId { get; set; }
    public int? InvitationGuestId { get; set; }
    public string? InvitationName { get; set; }
    public string? GuestName { get; set; }
    public List<FormAnswerDTO> Answers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GuestAnswersDTO
{
    public int? InvitationGuestId { get; set; }
    public List<FormAnswerDTO> Answers { get; set; } = new();
}

public class SubmitEventFormDTO
{
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string? InvitationCode { get; set; }

    public List<GuestAnswersDTO> GuestAnswers { get; set; } = new();
}