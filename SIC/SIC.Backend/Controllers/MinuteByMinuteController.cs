using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MinuteByMinuteController : GenericController<MinuteByMinute>
{
    private readonly IMinuteByMinuteUnitOfWork _minuteByMinuteUnitOfWork;

    public MinuteByMinuteController(
        IGenericUnitOfWork<MinuteByMinute> unitOfWork,
        IMinuteByMinuteUnitOfWork minuteByMinuteUnitOfWork)
        : base(unitOfWork)
    {
        _minuteByMinuteUnitOfWork = minuteByMinuteUnitOfWork;
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
