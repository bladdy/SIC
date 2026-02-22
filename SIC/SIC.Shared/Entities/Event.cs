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

    [Display(Name = "Fecha Limite")]
    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
    public DateTime? DeadLine { get; set; }

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
    public bool HasAlbum { get; set; } = false;
    public bool AlbumPublic { get; set; } = true;

    // 🔹 Álbum de imágenes del evento
    public ICollection<EventImage> Images { get; set; } = new List<EventImage>();

    // 🔹 Miniatura de la inivitacion
    public string? CoverImageUrl { get; set; }

    // Corver del album
    public string? CoverAlbumImageUrl { get; set; }

    [Display(Name = "URL del Confirmacion")]
    public string? UrlConfirmation { get; set; }

    public double CoverPositionX { get; set; }
    public double CoverPositionY { get; set; }
    public double CoverZoom { get; set; }

    public int? EventTypeId { get; set; }
    public EventType? EventType { get; set; }
    public ICollection<HistoryMessages> HistoryMessages { get; set; } = new List<HistoryMessages>();
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

    // 🔹 Total Jovenes invitados
    public int NumberYouths => Invitations?.Sum(a => a.NumberYouths) ?? 0;

    // 🔹 Total Niños invitados
    public int NumberChildren => Invitations?.Sum(a => a.NumberChildren) ?? 0;

    // 🔹 Adultos confirmados
    public int NumberAdultsConfirmed => Invitations?.Where(s => s.Status == Status.Attend).Sum(a => a.NumberConfirmedAdults) ?? 0;

    // 🔹 Jovenes confirmados
    public int NumberYouthsConfirmed => Invitations?.Where(s => s.Status == Status.Attend).Sum(a => a.NumberConfirmedYouths) ?? 0;

    // 🔹 Niños confirmados
    public int NumberChildrenConfirmed => Invitations?.Where(s => s.Status == Status.Attend).Sum(a => a.NumberConfirmedChildren) ?? 0;

    // 🔹 Adultos pendientes
    public int NumberAdultsPending => Invitations?.Where(s => s.Status == Status.Pending)
                                                  .Sum(a => a.NumberAdults) ?? 0;

    // 🔹 Niños pendientes
    public int NumberChildrenPending => Invitations?.Where(s => s.Status == Status.Pending)
                                                   .Sum(a => a.NumberChildren) ?? 0;

    // 🔹 Jovenes pendientes
    public int NumberYouthPending => Invitations?.Where(s => s.Status == Status.Pending)
                                                   .Sum(a => a.NumberYouths) ?? 0;

    // 🔹 Niños No asistirán
    public int NumberChildrenNotAttend => Invitations?.Where(s => s.Status == Status.NotAttend)
                                                   .Sum(a => a.NumberChildren) ?? 0;

    // 🔹 Jovenes No asistirán
    public int NumberYouthNotAttend => Invitations?.Where(s => s.Status == Status.NotAttend)
                                                   .Sum(a => a.NumberYouths) ?? 0;

    // 🔹 Adultos No asistiran
    public int NumberAdultsNotAttend => Invitations?.Where(s => s.Status == Status.NotAttend)
                                                  .Sum(a => a.NumberAdults) ?? 0;
}