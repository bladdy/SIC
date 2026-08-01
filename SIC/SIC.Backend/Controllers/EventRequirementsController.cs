using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventRequirementsController : GenericController<EventRequirement>
{
    private readonly IEventRequirementsUnitOfWork _unitOfWork;

    public EventRequirementsController(IGenericUnitOfWork<EventRequirement> genericUnitOfWork, IEventRequirementsUnitOfWork unitOfWork)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("bySection/{section}")]
    public async Task<IActionResult> GetBySectionAsync(string section)
    {
        var response = await _unitOfWork.GetBySectionAsync(section);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}
