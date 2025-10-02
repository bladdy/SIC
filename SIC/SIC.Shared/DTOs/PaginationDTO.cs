using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs;

public class PaginationDTO
{
    public int Id { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Filter { get; set; }
    public string? UserId { get; set; }
    public string? OrderBy { get; set; }
    public DateTime? Date { get; set; } = null;
    public int EventTypeId { get; set; } = 0;
    //Agregar: orderby fechas asc desc
    //Nuevos filtros: Por nombre de anfitriones, por eventos, usuarios, fechas
    //Nuevos filtros Mis eventos: Por nombre de anfitriones, por eventos, fechas
}