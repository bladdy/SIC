using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Response;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : GenericController<Event>
{
    private readonly IEventsUnitOfWork _eventsUnitOfWork;
    private readonly FtpStorageService _ftp;

    public EventsController(FtpStorageService ftp, IGenericUnitOfWork<Event> unitOfWork, IEventsUnitOfWork eventsUnitOfWork) : base(unitOfWork)
    {
        _ftp = ftp;
        _eventsUnitOfWork = eventsUnitOfWork;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        var response = await _eventsUnitOfWork.GetAsync();
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        var response = await _eventsUnitOfWork.GetAsync(pagination);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("totalRecords")]
    public override async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _eventsUnitOfWork.GetTotalRecordAsync(pagination);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("byUserId/{userId}")]
    public async Task<IActionResult> GetByUserIdAsync(string userId)
    {
        var response = await _eventsUnitOfWork.GetByUserIdAsync(userId);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("byCode/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        var response = await _eventsUnitOfWork.GetByCodeAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostFullAsync(Event events)
    {
        var action = await _eventsUnitOfWork.AddFullAsync(events);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutFullAsync(Event events)
    {
        var action = await _eventsUnitOfWork.UpdateFullAsync(events);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPost("upload-frontpage/{code}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PutFullAsync(
        IFormFile file, string code)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo inválido");

        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("La carpeta es obligatoria");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using var stream = file.OpenReadStream();
        var url = await _ftp.UploadImageAsync(stream, "FrontPages", fileName);
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest("Error al subir la imagen");
        ActionResponse<Event> action = new();
        var response = await _eventsUnitOfWork.GetByCodeAsync(code);
        if (response.Result != null)
        {
            response.Result.CoverImageUrl = url;
            action = await _eventsUnitOfWork.UpdateFullAsync(response.Result);
        }
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
}