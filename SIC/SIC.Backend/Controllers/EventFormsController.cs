using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventFormsController : GenericController<EventForm>
{
    private readonly IEventFormsUnitOfWork _unitOfWork;
    private readonly BoletaService _boletaService;

    public EventFormsController(IGenericUnitOfWork<EventForm> genericUnitOfWork, IEventFormsUnitOfWork unitOfWork, BoletaService boletaService)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _boletaService = boletaService;
    }

    [HttpGet("byEventCode/{eventCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEventCodeAsync(string eventCode)
    {
        var response = await _unitOfWork.GetByEventCodeAsync(eventCode);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPost("save")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,WeddingPlanner,User")]
    public async Task<IActionResult> SaveAsync(SaveEventFormDTO dto)
    {
        if (dto.Formulario == null || dto.Formulario.Preguntas.Count == 0)
            return BadRequest("El formulario debe contener al menos una pregunta.");

        var response = await _unitOfWork.SaveAsync(dto.EventId, dto);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpGet("responses/{eventCode}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,WeddingPlanner,User")]
    public async Task<IActionResult> GetResponsesByEventAsync(string eventCode)
    {
        var response = await _unitOfWork.GetResponsesByEventAsync(eventCode);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpGet("byInvitation/{invitationCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetResponseByInvitationAsync(string invitationCode)
    {
        var response = await _unitOfWork.GetResponseByInvitationAsync(invitationCode);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpGet("responses/{eventCode}/pdf")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,WeddingPlanner,User")]
    public async Task<IActionResult> GetResponsesPdfAsync(string eventCode, string? evento)
    {
        var response = await _unitOfWork.GetResponsesByEventAsync(eventCode);
        if (!response.Success)
            return BadRequest(response.Message);

        var responses = response.Result?.ToList();
        if (responses == null || responses.Count == 0)
            return NotFound("No hay respuestas para generar el PDF.");

        var pdfBytes = _boletaService.GenerarCuestionarioPdf(string.IsNullOrWhiteSpace(evento) ? eventCode : evento, responses);

        return File(pdfBytes, "application/pdf", $"respuestas-{eventCode}.pdf");
    }

    [HttpDelete("responses/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,WeddingPlanner,User")]
    public async Task<IActionResult> ResetResponseAsync(int id)
    {
        var response = await _unitOfWork.ResetResponseAsync(id);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPost("respond")]
    [AllowAnonymous]
    public async Task<IActionResult> RespondAsync(SubmitEventFormDTO dto)
    {
        var response = await _unitOfWork.SaveResponseAsync(dto);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}