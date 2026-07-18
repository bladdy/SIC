using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Data;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MbMProvidersController : GenericController<MbMProvider>
{
    private readonly IMbMProviderUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public MbMProvidersController(
        IGenericUnitOfWork<MbMProvider> genericUnitOfWork,
        IMbMProviderUnitOfWork unitOfWork,
        DataContext context)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    [HttpGet("byActivityId/{activityId}")]
    public async Task<IActionResult> GetByActivityIdAsync(int activityId)
    {
        var response = await _unitOfWork.GetByActivityIdAsync(activityId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] MbMProviderDTO dto)
    {
        var response = await _unitOfWork.GetAsync(id);
        if (!response.Success || response.Result == null)
            return NotFound("El proveedor no existe.");

        var provider = response.Result;
        provider.Name = dto.Name;
        provider.Contact = dto.Contact;
        provider.Service = dto.Service;
        provider.Status = dto.Status;
        provider.Cost = dto.Cost;

        var updateResponse = await _unitOfWork.UpdateAsync(provider);
        if (!updateResponse.Success)
            return BadRequest(updateResponse.Message);
        return Ok(updateResponse.Result);
    }

    [HttpPost("ByActivityId/{activityId}")]
    public async Task<IActionResult> PostByActivityIdAsync(int activityId, [FromBody] MbMProviderDTO dto)
    {
        var activity = await _context.MbMActivities.FindAsync(activityId);
        if (activity == null)
            return BadRequest("La actividad no existe.");

        var provider = new MbMProvider
        {
            Name = dto.Name,
            Contact = dto.Contact,
            Service = dto.Service,
            Status = dto.Status,
            Cost = dto.Cost,
            MbMActivityId = activityId
        };

        var response = await _unitOfWork.AddAsync(provider);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}
