using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
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

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetAsync(int id)
    {
        var response = await _eventsUnitOfWork.GetAsync(id);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }
}