using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities;

public class InvitationEntry
{
    public int Id { get; set; }

    [Display(Name = "Código de invitacion")]
    public string Code { get; set; } = null!;

    public int InvitationId { get; set; }
    public Invitation? Invitation { get; set; } = null!;
    public int EventId { get; set; }
    public Event? Event { get; set; } = null!;

    [Display(Name = "Cantidad de Adultos")]
    [Range(0, int.MaxValue)]
    public int AdultsEntered { get; set; }

    [Display(Name = "Cantidad de Jovenes")]
    [Range(0, int.MaxValue)]
    public int YouthsEntered { get; set; }

    [Display(Name = "Cantidad de Niños")]
    [Range(0, int.MaxValue)]
    public int ChildrenEntered { get; set; }

    [Display(Name = "Fecha y Hora de Entrada")]
    public DateTime EntryDateTime { get; set; } = DateTime.UtcNow;

    [Display(Name = "Código QR Escaneado")]
    public string QrCode { get; set; } = null!;
}