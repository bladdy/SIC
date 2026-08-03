using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventRequirementImagesController : GenericController<EventRequirementImage>
{
    private readonly IEventRequirementImagesUnitOfWork _unitOfWork;
    private readonly FtpStorageService _ftpService;

    public EventRequirementImagesController(
        IGenericUnitOfWork<EventRequirementImage> genericUnitOfWork,
        IEventRequirementImagesUnitOfWork unitOfWork,
        FtpStorageService ftpService)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ftpService = ftpService;
    }

    [HttpGet("byAnswerId/{answerId}")]
    public async Task<IActionResult> GetByAnswerIdAsync(int answerId)
    {
        var response = await _unitOfWork.GetByAnswerIdAsync(answerId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpGet("byEventId/{eventId}")]
    public async Task<IActionResult> GetByEventIdAsync(int eventId)
    {
        var response = await _unitOfWork.GetByEventIdAsync(eventId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPost("upload/{requirementAnswerId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAsync(int requirementAnswerId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No se envió ningún archivo.");

        var folder = $"event-requirements/{requirementAnswerId}";
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using var stream = file.OpenReadStream();
        var url = await _ftpService.UploadImageAsync(stream, folder, fileName);

        var entity = new EventRequirementImage
        {
            RequirementAnswerId = requirementAnswerId,
            FileName = fileName,
            OriginalName = file.FileName,
            Path = url,
            Order = 0
        };

        var response = await _unitOfWork.AddAsync(entity);
        if (!response.Success)
            return BadRequest(response.Message);

        return Ok(response.Result);
    }

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete(int id)
    {
        var getResponse = await _unitOfWork.GetByIdWithAnswerAsync(id);
        if (!getResponse.Success)
            return BadRequest(getResponse.Message);
        if (getResponse.Result == null)
            return NotFound("Imagen no encontrada.");

        var image = getResponse.Result;
        if (image.RequirementAnswer != null)
        {
            var folder = $"event-requirements/{image.RequirementAnswer.EventId}";
            try
            {
                await _ftpService.DeleteFileAsync(folder, image.FileName);
            }
            catch
            {
                // Si el archivo FTP no se puede borrar, continuamos con el borrado en BD.
            }
        }

        var response = await _unitOfWork.DeleteAsync(id);
        if (!response.Success)
            return BadRequest(response.Message);

        return Ok(response.Result);
    }
}
