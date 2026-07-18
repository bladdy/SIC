using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Data;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MbMActivitiesController : GenericController<MbMActivity>
{
    private readonly IMbMActivityUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public MbMActivitiesController(
        IGenericUnitOfWork<MbMActivity> genericUnitOfWork,
        IMbMActivityUnitOfWork unitOfWork,
        DataContext context)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    [HttpGet("withDetails/{id}")]
    public async Task<IActionResult> GetWithDetailsAsync(int id)
    {
        var response = await _unitOfWork.GetWithDetailsAsync(id);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpGet("byMinuteByMinuteId/{minuteByMinuteId}")]
    public async Task<IActionResult> GetByMinuteByMinuteIdAsync(int minuteByMinuteId)
    {
        var response = await _unitOfWork.GetByMinuteByMinuteIdAsync(minuteByMinuteId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] MbMActivityDTO dto)
    {
        var response = await _unitOfWork.GetAsync(id);
        if (!response.Success || response.Result == null)
            return NotFound("La actividad no existe.");

        var activity = response.Result;
        activity.Title = dto.Title;
        activity.Description = dto.Description;
        activity.StartTime = dto.StartTime;
        activity.EndTime = dto.EndTime;
        activity.Status = dto.Status;
        activity.Priority = dto.Priority;
        activity.Location = dto.Location;
        activity.Notes = dto.Notes;

        var updateResponse = await _unitOfWork.UpdateAsync(activity);
        if (!updateResponse.Success)
            return BadRequest(updateResponse.Message);
        return Ok(updateResponse.Result);
    }

    [HttpPost("ByMinuteByMinuteId/{minuteByMinuteId}")]
    public async Task<IActionResult> PostByMinuteByMinuteIdAsync(int minuteByMinuteId, [FromBody] MbMActivityDTO dto)
    {
        var mbm = await _context.MinuteByMinutes.FindAsync(minuteByMinuteId);
        if (mbm == null)
            return BadRequest("El Minute by Minute no existe.");

        var activity = new MbMActivity
        {
            Title = dto.Title,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status,
            Priority = dto.Priority,
            Location = dto.Location,
            Notes = dto.Notes,
            MinuteByMinuteId = minuteByMinuteId
        };

        var response = await _unitOfWork.AddAsync(activity);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}
