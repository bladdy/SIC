using SIC.Shared.Enums;
using SIC.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities;

public class Event : IEntityWithName
{
    public int Id { get; set; }
    public string? Code { get; set; }

    [Display(Name = "Titulo")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;

    [Display(Name = "Sub-Titulo")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string SubTitle { get; set; } = null!;

    [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
    [Display(Name = "Fecha")]
    public DateTime Date { get; set; } = DateTime.Now;

    [DisplayFormat(DataFormatString = "{0:hh:mm}")]
    [Display(Name = "Hora")]
    public TimeSpan Time { get; set; }

    public string Url { get; set; } = null!;
    public string Host { get; set; } = null!;
    public string HostPhone { get; set; } = null!;
    public string? Planner { get; set; }
    public string? PlannerPhone { get; set; }
    public int? EventTypeId { get; set; }
    public EventType? EventType { get; set; } = null!;

    public ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    [Display(Name = "Estado")]
    public Status Status { get; set; }

    [Display(Name = "Cantidad de invitados")]
    public int Guests => Invitations == null || Invitations.Count == 0 ? 0 : Invitations.Count;

    public User? User { get; set; }
    public string? UserId { get; set; }

    //public DateTime CreatedDate { get; set; } = DateTime.Now;
    //[Display(Name = "Confirmaciones")]
    //public int StatesNumber => States == null || States.Count == 0 ? 0 : States.Count;
    //[Display(Name = "Pendientes")]
    //public int StatesNumber => States == null || States.Count == 0 ? 0 : States.Count;
    //[Display(Name = "Adultos")]
    //public int StatesNumber => States == null || States.Count == 0 ? 0 : States.Count;
    //[Display(Name = "Niños")]
    //public int StatesNumber => States == null || States.Count == 0 ? 0 : States.Count;
    //[Display(Name = "Adultos Confirmados")]
    //public int StatesNumber => States == null || States.Count == 0 ? 0 : States.Count;
    //[Display(Name = "Niños Confirmados")]
    //public int StatesNumber => States == null || States.Count == 0 ? 0 : States.Count;
}