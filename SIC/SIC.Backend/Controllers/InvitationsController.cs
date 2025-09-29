using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationsController : GenericController<Invitation>
{
    private readonly IInvitationUnitOfWork _invitationUnitOfWork;

    public InvitationsController(IGenericUnitOfWork<Invitation> unitOfWork, IInvitationUnitOfWork invitationUnitOfWork) : base(unitOfWork)
    {
        _invitationUnitOfWork = invitationUnitOfWork;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        var response = await _invitationUnitOfWork.GetAsync();
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("byCode/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        var response = await _invitationUnitOfWork.GetByCodeAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostFullAsync(Invitation invitation)
    {
        var action = await _invitationUnitOfWork.AddFullAsync(invitation);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutFullAsync(Invitation invitation)
    {
        var action = await _invitationUnitOfWork.UpdateFullAsync(invitation);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
}