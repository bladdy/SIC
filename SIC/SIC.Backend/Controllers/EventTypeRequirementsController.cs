using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventTypeRequirementsController : GenericController<EventTypeRequirement>
{
    private readonly IEventTypeRequirementsUnitOfWork _unitOfWork;

    public EventTypeRequirementsController(IGenericUnitOfWork<EventTypeRequirement> genericUnitOfWork, IEventTypeRequirementsUnitOfWork unitOfWork)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("byEventTypeId/{eventTypeId}")]
    public async Task<IActionResult> GetByEventTypeIdAsync(int eventTypeId)
    {
        var response = await _unitOfWork.GetByEventTypeIdAsync(eventTypeId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpGet("exists/{eventTypeId}/{requirementId}")]
    public async Task<IActionResult> ExistsAsync(int eventTypeId, int requirementId)
    {
        var response = await _unitOfWork.ExistsAsync(eventTypeId, requirementId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}
