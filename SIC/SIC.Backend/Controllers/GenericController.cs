using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Interfaces;

namespace SIC.Backend.Controllers;

public class GenericController<T> : Controller where T : class
{
    private readonly IGenericUnitOfWork<T> _unitOfWork;

    public GenericController(IGenericUnitOfWork<T> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> Get(int id)
    {
        var response = await _unitOfWork.GetAsync(id);
        if (!response.Success)
            return BadRequest(response.Message);
        if (response.Result == null)
            return NotFound(response.Message);
        return Ok(response.Result);
    }
    [HttpGet]
    public virtual async Task<IActionResult> Get()
    {
        var response = await _unitOfWork.GetAsync();
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
    [HttpPost]
    public virtual async Task<IActionResult> Post([FromBody] T entity)
    {
        var response = await _unitOfWork.AddAsync(entity);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
    [HttpPut]
    public virtual async Task<IActionResult> Put([FromBody] T entity)
    {
        var response = await _unitOfWork.UpdateAsync(entity);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var response = await _unitOfWork.DeleteAsync(id);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}
