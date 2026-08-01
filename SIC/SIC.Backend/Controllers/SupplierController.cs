using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupplierController : GenericController<Supplier>
{
    public SupplierController(IGenericUnitOfWork<Supplier> unitOfWork) : base(unitOfWork)
    {
    }

    [HttpGet("byCode/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        //var response = await _invitationUnitOfWork.GetByCodeAsync(code);
        //if (response.Success)
        //{
        //    return Ok(response.Result);
        //}
        return NotFound();
    }
}