using SIC.Shared.Enums;
using SIC.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SIC.Shared.Entities;

public class Event : IEntityWithName
{
    public int Id { get; set; }
    public string? Code { get; set; }

    [Display(Name = "Título")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;

    [Display(Name = "Subtítulo")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string SubTitle { get; set; } = null!;

    [Display(Name = "Fecha")]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
    public DateTime Date { get; set; } = DateTime.Now;

    [Display(Name = "Hora")]
    [DisplayFormat(DataFormatString = "{0:hh:mm}")]
    public TimeSpan Time { get; set; }

    [Display(Name = "Ubicación")]
    public string? Ubication { get; set; }

    [Display(Name = "URL del evento")]
    public string? Url { get; set; }

    [Display(Name = "Anfitrión")]
    public string Host { get; set; } = null!;

    [Display(Name = "Teléfono del anfitrión")]
    public string HostPhone { get; set; } = null!;

    public string? Planner { get; set; }
    public string? PlannerPhone { get; set; }

    public int? EventTypeId { get; set; }
    public EventType? EventType { get; set; }

    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    [Display(Name = "Estado")]
    public Status Status { get; set; }

    [Display(Name = "Cantidad de invitados")]
    public int Guests => Invitations?.Count ?? 0;

    public Message? Message { get; set; }

    public User? User { get; set; }
    public string? UserId { get; set; }

    // 🔹 Invitaciones Totales
    public int InvitationsNumbers => Invitations?.Count ?? 0;

    // 🔹 Invitaciones Confirmadas
    public int Confirmations => Invitations?.Count(s => s.Status == Status.Attend) ?? 0;

    // 🔹 Invitaciones Pendientes
    public int Pending => Invitations?.Count(s => s.Status == Status.Pending) ?? 0;

    // 🔹 Total Adultos invitados
    public int NumberAdults => Invitations?.Sum(a => a.NumberAdults) ?? 0;

    // 🔹 Total Niños invitados
    public int NumberChildren => Invitations?.Sum(a => a.NumberChildren) ?? 0;

    // 🔹 Adultos confirmados
    public int NumberAdultsConfirmed => Invitations?.Where(s => s.Status == Status.Attend).Sum(a => a.NumberConfirmedAdults) ?? 0;

    // 🔹 Niños confirmados
    public int NumberChildrenConfirmed => Invitations?.Where(s => s.Status == Status.Attend).Sum(a => a.NumberConfirmedChildren) ?? 0;

    // 🔹 Adultos pendientes
    public int NumberAdultsPending => Invitations?.Where(s => s.Status == Status.Pending)
                                                  .Sum(a => a.NumberAdults) ?? 0;

    // 🔹 Niños pendientes
    public int NumberChildrenPending => Invitations?.Where(s => s.Status == Status.Pending)
                                                   .Sum(a => a.NumberChildren) ?? 0;

    // 🔹 Niños No asistirán
    public int NumberChildrenNotAttend => Invitations?.Where(s => s.Status == Status.NotAttend)
                                                   .Sum(a => a.NumberChildren) ?? 0;

    // 🔹 Adultos No asistiran
    public int NumberAdultsNotAttend => Invitations?.Where(s => s.Status == Status.NotAttend)
                                                  .Sum(a => a.NumberAdults) ?? 0;
}