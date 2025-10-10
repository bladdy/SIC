using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanItemController : GenericController<PlanItem>
{
    private readonly IPlanItemUnitOfWork _planItemUnitOfWork;

    public PlanItemController(IGenericUnitOfWork<PlanItem> unitOfWork, IPlanItemUnitOfWork planItemUnitOfWork) : base(unitOfWork)
    {
        _planItemUnitOfWork = planItemUnitOfWork;
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _planItemUnitOfWork.GetAsync(pagination);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpGet("totalRecordAsync")]
    public override async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _planItemUnitOfWork.GetTotalRecordAsync(pagination);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        var action = await _planItemUnitOfWork.GetAsync();
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpGet("Item-By-Plan/{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var action = await _planItemUnitOfWork.GetByIdAsync(id);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpPost("full/{planId}")]
    public async Task<IActionResult> PostFullAsync(List<int> items, int planId)
    {
        var action = await _planItemUnitOfWork.AddOrUpdateFullAsync(items, planId);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
}