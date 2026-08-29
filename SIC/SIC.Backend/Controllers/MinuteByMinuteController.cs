using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MinuteByMinuteController : GenericController<MinuteByMinute>
{
    private readonly IMinuteByMinuteUnitOfWork _minuteByMinuteUnitOfWork;
    private readonly BoletaService _boletaService;

    public MinuteByMinuteController(
        IGenericUnitOfWork<MinuteByMinute> unitOfWork,
        IMinuteByMinuteUnitOfWork minuteByMinuteUnitOfWork,
        BoletaService boletaService)
        : base(unitOfWork)
    {
        _minuteByMinuteUnitOfWork = minuteByMinuteUnitOfWork;
        _boletaService = boletaService;
    }

    [HttpGet("generatedpdf")]
    public async Task<IActionResult> GetMinutoAMinutoPdf(string eventId, string evento)
    {
        try
        {
            var response = await _minuteByMinuteUnitOfWork.GetByEventCodeAsync(eventId);
            var minuteByMinute = response.Result;

            if (minuteByMinute == null || minuteByMinute.Activities == null || !minuteByMinute.Activities.Any())
                return NotFound("No hay actividades para generar el PDF.");

            var titulo = "Minuto a Minuto";
            var nombre = string.IsNullOrWhiteSpace(evento) ? minuteByMinute.Event?.Name : evento;
            var tipoEvento = minuteByMinute.Event?.EventType?.Name;

            var pdfBytes = _boletaService.GenerarMinutoAMinutoPdf(titulo, nombre ?? "", tipoEvento, minuteByMinute.Activities.ToList());

            return File(
                pdfBytes,
                "application/pdf",
                $"minuto-a-minuto-{eventId}.pdf"
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("byEventId/{eventId}")]
    public async Task<IActionResult> GetByEventIdAsync(int eventId)
    {
        var response = await _minuteByMinuteUnitOfWork.GetByEventIdAsync(eventId);
        if (!response.Success)
            return BadRequest(response.Message);
        if (response.Result == null)
            return NotFound(response.Message);
        return Ok(response.Result);
    }

    [HttpGet("byEventCode/{code}")]
    public async Task<IActionResult> GetByEventCodeAsync(string code)
    {
        var response = await _minuteByMinuteUnitOfWork.GetByEventCodeAsync(code);
        if (!response.Success)
            return BadRequest(response.Message);
        if (response.Result == null)
            return NotFound(response.Message);
        return Ok(response.Result);
    }

    [HttpPost("byEventId/{eventId}")]
    public async Task<IActionResult> PostByEventIdAsync(int eventId, [FromBody] MinuteByMinuteDTO dto)
    {
        var minuteByMinute = new MinuteByMinute
        {
            Title = dto.Title,
            IsPublic = dto.IsPublic
        };

        var response = await _minuteByMinuteUnitOfWork.CreateForEventAsync(minuteByMinute, eventId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}