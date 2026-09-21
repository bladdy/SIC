using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIC.Shared.Entities;

public class EventForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int EventId { get; set; }

    [ForeignKey("EventId")]
    public Event? Event { get; set; }

    [Display(Name = "Título")]
    public string? Title { get; set; }

    [Display(Name = "Definición del formulario")]
    public string? FormJson { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Fecha de Creación")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Fecha de Modificación")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}