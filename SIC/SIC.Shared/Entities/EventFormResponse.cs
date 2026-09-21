using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities;

public class EventFormResponse
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event? Event { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int InvitationId { get; set; }

    [ForeignKey("InvitationId")]
    public Invitation? Invitation { get; set; }

    public int? InvitationGuestId { get; set; }

    [ForeignKey("InvitationGuestId")]
    public InvitationGuest? InvitationGuest { get; set; }

    [Display(Name = "Invitación")]
    public string? InvitationName { get; set; }

    [Display(Name = "Invitado")]
    public string? GuestName { get; set; }

    [Display(Name = "Respuestas")]
    public string? AnswersJson { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Fecha de Modificación")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}