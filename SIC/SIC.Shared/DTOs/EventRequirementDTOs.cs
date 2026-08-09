using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class EventRequirementDTO
{
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [Display(Name = "Descripción")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Display(Name = "Sección")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    [MaxLength(100)]
    public string Section { get; set; } = null!;

    [Display(Name = "Tipo de Entrada")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public RequirementInputType InputType { get; set; }

    [Display(Name = "Placeholder")]
    [MaxLength(200)]
    public string? Placeholder { get; set; }

    [Display(Name = "Obligatorio")]
    public bool IsRequired { get; set; }

    [Display(Name = "Mínimo de Imágenes")]
    [Range(0, 100)]
    public int MinImages { get; set; }

    [Display(Name = "Máximo de Imágenes")]
    [Range(0, 100)]
    public int MaxImages { get; set; }

    [Display(Name = "Orden")]
    public int SortOrder { get; set; }

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;
}

public class EventTypeRequirementDTO
{
    public int Id { get; set; }

    [Display(Name = "Tipo de Evento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int EventTypeId { get; set; }

    [Display(Name = "Requisito")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int RequirementId { get; set; }

    [Display(Name = "Orden")]
    public int SortOrder { get; set; }

    public string? RequirementName { get; set; }
    public string? RequirementSection { get; set; }
    public RequirementInputType? RequirementInputType { get; set; }
    public bool? RequirementIsRequired { get; set; }
    public bool? RequirementIsActive { get; set; }
    public string? RequirementPlaceholder { get; set; }
    public int RequirementMinImages { get; set; }
    public int RequirementMaxImages { get; set; }
}

public class EventRequirementAnswerDTO
{
    public int Id { get; set; }

    [Display(Name = "Evento")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int EventId { get; set; }

    [Display(Name = "Requisito")]
    [Required(ErrorMessage = "El campo {0} es obligatorio.")]
    public int RequirementId { get; set; }

    [Display(Name = "Valor")]
    [MaxLength(2000)]
    public string? Value { get; set; }
}

public class EventRequirementFormDTO
{
    public int EventId { get; set; }
    public int EventTypeId { get; set; }
    public string? EventName { get; set; }
    public string? EventTypeName { get; set; }
    public List<EventTypeRequirementDTO> Requirements { get; set; } = new();
    public List<EventRequirementAnswerDTO> Answers { get; set; } = new();
    public List<EventRequirementImageDTO> Images { get; set; } = new();
}

public class EventRequirementImageDTO
{
    public int Id { get; set; }
    public int RequirementAnswerId { get; set; }
    public int RequirementId { get; set; }
    public string FileName { get; set; } = null!;
    public string OriginalName { get; set; } = null!;
    public string Path { get; set; } = null!;
    public int Order { get; set; }
}

public class SaveFormResponseDTO
{
    public List<EventRequirementAnswerDTO> Answers { get; set; } = new();
    public List<EventRequirementImageDTO> Images { get; set; } = new();
}

public class SaveAnswersDTO
{
    public int EventId { get; set; }
    public List<EventRequirementAnswerDTO> Answers { get; set; } = new();
}