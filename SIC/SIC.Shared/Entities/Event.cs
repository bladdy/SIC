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
    public string Code { get; set; } = null!;

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

    [Display(Name = "Estado")]
    public Status Status { get; set; }
}