using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : GenericController<Event>
{
    private readonly IEventsUnitOfWork _eventsUnitOfWork;

    public EventsController(IGenericUnitOfWork<Event> unitOfWork, IEventsUnitOfWork eventsUnitOfWork) : base(unitOfWork)
    {
        _eventsUnitOfWork = eventsUnitOfWork;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        var response = await _eventsUnitOfWork.GetAsync();
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        var response = await _eventsUnitOfWork.GetAsync(pagination);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("totalRecords")]
    public override async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _eventsUnitOfWork.GetTotalRecordAsync(pagination);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("byUserId/{userId}")]
    public async Task<IActionResult> GetByUserIdAsync(string userId)
    {
        var response = await _eventsUnitOfWork.GetByUserIdAsync(userId);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("byCode/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        var response = await _eventsUnitOfWork.GetByCodeAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostFullAsync(Event events)
    {
        var action = await _eventsUnitOfWork.AddFullAsync(events);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutFullAsync(Event events)
    {
        var action = await _eventsUnitOfWork.UpdateFullAsync(events);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
}