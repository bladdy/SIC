using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationEntryController : GenericController<InvitationEntry>
{
    private readonly IGenericUnitOfWork<InvitationEntry> _genericUnit;
    private readonly IInvitationEntryUnitOfWork _invitationEntryUnitOfWork;

    public InvitationEntryController(IGenericUnitOfWork<InvitationEntry> genericUnit, IInvitationEntryUnitOfWork invitationEntryUnitOfWork) : base(genericUnit)
    {
        _genericUnit = genericUnit;
        _invitationEntryUnitOfWork = invitationEntryUnitOfWork;
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        var response = await _invitationEntryUnitOfWork.GetAsync(pagination);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("totalRecords")]
    public override async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _invitationEntryUnitOfWork.GetTotalRecordAsync(pagination);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("byCode/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        var response = await _invitationEntryUnitOfWork.GetByCodeAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostFullAsync(InvitationEntry invitationEntry)
    {
        var action = await _invitationEntryUnitOfWork.AddFullAsync(invitationEntry);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutFullAsync(InvitationEntry invitationEntry)
    {
        var action = await _invitationEntryUnitOfWork.UpdateFullAsync(invitationEntry);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
}