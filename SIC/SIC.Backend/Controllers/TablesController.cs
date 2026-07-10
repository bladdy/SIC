using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TablesController : GenericController<TablesEvents>
{
    private readonly ITablesEventsUnitOfWork _tablesEventsUnit;

    public TablesController(IGenericUnitOfWork<TablesEvents> unitOfWork, ITablesEventsUnitOfWork tablesEventsUnit) : base(unitOfWork)
    {
        _tablesEventsUnit = tablesEventsUnit;
    }

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetAsync(int id)
    {
        var response = await _tablesEventsUnit.GetAsync(id);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("tablesbycode/{code}")]
    public async Task<IActionResult> GetTablesByCodeAsync(string code)
    {
        var response = await _tablesEventsUnit.GetTablesByCodeAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        var response = await _tablesEventsUnit.GetAsync();
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        var response = await _tablesEventsUnit.GetAsync(pagination);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("totalRecords")]
    public override async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _tablesEventsUnit.GetTotalRecordAsync(pagination);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostFullAsync(CreateOrEditTablesDto table)
    {
        var action = await _tablesEventsUnit.AddFullAsync(table);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutFullAsync(CreateOrEditTablesDto table)
    {
        var action = await _tablesEventsUnit.UpdateFullAsync(table);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPost("Assign")]
    public async Task<IActionResult> PostFullAsync(AssignTablesDto table)
    {
        var action = await _tablesEventsUnit.AssignTablesAsync(table);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpPost("Generate")]
    public async Task<IActionResult> PostFullAsync(GenerateTablesDto table)
    {
        var action = await _tablesEventsUnit.GenerateTablesAsync(table);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteInvitatonFromTablesAsync(int id)
    {
        var action = await _tablesEventsUnit.DeleteInvitatonFromTablesAsync(id);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }
}