using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Data;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MbMTasksController : GenericController<MbMTask>
{
    private readonly IMbMTaskUnitOfWork _unitOfWork;
    private readonly DataContext _context;

    public MbMTasksController(
        IGenericUnitOfWork<MbMTask> genericUnitOfWork,
        IMbMTaskUnitOfWork unitOfWork,
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
    public async Task<IActionResult> PutAsync(int id, [FromBody] MbMTaskDTO dto)
    {
        var response = await _unitOfWork.GetAsync(id);
        if (!response.Success || response.Result == null)
            return NotFound("La tarea no existe.");

        var task = response.Result;
        task.Title = dto.Title;
        task.IsCompleted = dto.IsCompleted;
        task.AssignedTo = dto.AssignedTo;
        task.ResponsiblePhone = dto.ResponsiblePhone;
        task.Motivo = dto.Motivo;

        var updateResponse = await _unitOfWork.UpdateAsync(task);
        if (!updateResponse.Success)
            return BadRequest(updateResponse.Message);
        return Ok(updateResponse.Result);
    }

    [HttpPost("ByActivityId/{activityId}")]
    public async Task<IActionResult> PostByActivityIdAsync(int activityId, [FromBody] MbMTaskDTO dto)
    {
        var activity = await _context.MbMActivities.FindAsync(activityId);
        if (activity == null)
            return BadRequest("La actividad no existe.");

        var task = new MbMTask
        {
            Title = dto.Title,
            IsCompleted = dto.IsCompleted,
            AssignedTo = dto.AssignedTo,
            ResponsiblePhone = dto.ResponsiblePhone,
            MbMActivityId = activityId,
            Motivo = dto.Motivo
        };

        var response = await _unitOfWork.AddAsync(task);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPut("toggle/{taskId}")]
    public async Task<IActionResult> ToggleCompleteAsync(int taskId)
    {
        var response = await _unitOfWork.ToggleCompleteAsync(taskId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}