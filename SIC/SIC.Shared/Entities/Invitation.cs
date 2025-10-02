using SIC.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities;

public class Invitation
{
    public int Id { get; set; }
    public string? Code { get; set; }//Auto generado

    [Display(Name = "Nombre")]
    [MaxLength(100, ErrorMessage = "El campo {0} debe tener máximo {1} caracteres.")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public string Name { get; set; } = null!;

    [Display(Name = "Correo Electrónico")]
    public string? Email { get; set; }

    [Display(Name = "Numero de Telefono")]
    public string PhoneNumber { get; set; } = null!;

    [Display(Name = "Numero de Adultos")]
    public int NumberAdults { get; set; }

    [Display(Name = "Numero de Niños")]
    public int NumberChildren { get; set; }

    [Display(Name = "Numero de Adultos Confirmados")]
    public int NumberConfirmedAdults { get; set; }

    [Display(Name = "Numero de Niños Confirmados")]
    public int NumberConfirmedChildren { get; set; }

    [Display(Name = "Estado")]
    public Status Status { get; set; }

    [Display(Name = "Mesa")]
    public string? Table { get; set; }

    [Display(Name = "Comentarios")]
    public string? Comments { get; set; }

    public DateTime SentDate { get; set; }
    public DateTime? ConfirmationDate { get; set; }
    public int EventId { get; set; }
    public Event? Event { get; set; } = null!;
}