using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;


[ApiController]
[Route("api/[controller]")]
public class PlanController : GenericController<Plan>
{
    private readonly IPlanUnitOfWork _planUnitOfWork;

    public PlanController(IGenericUnitOfWork<Plan> unitOfWork, IPlanUnitOfWork planUnitOfWork) : base(unitOfWork)
    {
        _planUnitOfWork = planUnitOfWork;
    }
    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        var action = await _planUnitOfWork.GetAsync();
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }
    [HttpGet("{id}")]
    public override async Task<IActionResult> GetAsync(int id) 
    {
        var action = await _planUnitOfWork.GetAsync(id);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }
}
