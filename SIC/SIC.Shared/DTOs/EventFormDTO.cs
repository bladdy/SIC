using Microsoft.AspNetCore.Http;
using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class EventFormDTO
{
    public int? Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    public string? SubTitle { get; set; } 
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string? Ubication { get; set; }
    public string? Url { get; set; }
    public string? Host { get; set; } 
    public string? HostPhone { get; set; } 
    public string? Planner { get; set; }
    public string? PlannerPhone { get; set; }
    public bool HasAlbum { get; set; }

    // 🔥 IMAGEN DE PORTADA
    public IFormFile? CoverImage { get; set; }

    public int? EventTypeId { get; set; }
    public string? UserId { get; set; }
    public string? Code { get; set; }
    public Status Status { get; set; }
}