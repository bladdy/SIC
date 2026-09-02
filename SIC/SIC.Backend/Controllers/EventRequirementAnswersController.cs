using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventRequirementAnswersController : GenericController<EventRequirementAnswer>
{
    private readonly IEventRequirementAnswersUnitOfWork _unitOfWork;
    private readonly FtpStorageService _ftpService;

    public EventRequirementAnswersController(
        IGenericUnitOfWork<EventRequirementAnswer> genericUnitOfWork,
        IEventRequirementAnswersUnitOfWork unitOfWork,
        FtpStorageService ftpService)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ftpService = ftpService;
    }

    [HttpGet("byEventId/{eventId}")]
    public async Task<IActionResult> GetByEventIdAsync(int eventId)
    {
        var response = await _unitOfWork.GetByEventIdAsync(eventId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveAllAsync([FromBody] SaveAnswersDTO dto)
    {
        var response = await _unitOfWork.SaveAllAsync(dto.EventId, dto.Answers);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPost("save-form")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SaveFormAsync()
    {
        if (!int.TryParse(Request.Form["eventId"], out var eventId))
            return BadRequest("eventId inválido.");

        var answersJson = Request.Form["answers"].FirstOrDefault();
        if (string.IsNullOrEmpty(answersJson))
            return BadRequest("answers no proporcionado.");

        var answersList = JsonSerializer.Deserialize<List<EventRequirementAnswerDTO>>(answersJson);
        if (answersList == null)
            return BadRequest("El formato de 'answers' no es válido.");

        var filesByReq = new Dictionary<int, List<(IFormFile File, int Order)>>();
        foreach (var file in Request.Form.Files)
        {
            var match = Regex.Match(file.Name, @"^images_(\d+)_(\d+)$");
            if (match.Success)
            {
                var reqId = int.Parse(match.Groups[1].Value);
                var order = int.Parse(match.Groups[2].Value);
                if (!filesByReq.ContainsKey(reqId))
                    filesByReq[reqId] = new List<(IFormFile, int)>();
                filesByReq[reqId].Add((file, order));
            }
        }

        var existingImagesJson = Request.Form["existingImages"].FirstOrDefault();
        var existingImages = string.IsNullOrEmpty(existingImagesJson)
            ? new List<EventRequirementImageDTO>()
            : JsonSerializer.Deserialize<List<EventRequirementImageDTO>>(existingImagesJson) ?? new List<EventRequirementImageDTO>();

        var imageDtos = new List<EventRequirementImageDTO>(existingImages);
        foreach (var (reqId, fileEntries) in filesByReq)
        {
            foreach (var (file, order) in fileEntries)
            {
                if (file.Length > 2 * 1024 * 1024)
                    continue;

                var folder = $"event-requirements/{eventId}";
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                using var stream = file.OpenReadStream();
                var url = await _ftpService.UploadRawImageAsync(stream, folder, fileName);

                imageDtos.Add(new EventRequirementImageDTO
                {
                    RequirementId = reqId,
                    FileName = fileName,
                    OriginalName = file.FileName,
                    Path = url,
                    Order = order
                });
            }
        }

        var response = await _unitOfWork.SaveFormAsync(eventId, answersList, imageDtos);
        if (!response.Success)
            return BadRequest(response.Message);

        return Ok(response.Result);
    }

    [HttpDelete("clear-field/{eventId}/{requirementId}")]
    public async Task<IActionResult> ClearFieldAsync(int eventId, int requirementId)
    {
        var getResponse = await _unitOfWork.GetByEventAndRequirementAsync(eventId, requirementId);
        if (!getResponse.Success)
            return BadRequest(getResponse.Message);

        if (getResponse.Result != null)
        {
            foreach (var img in getResponse.Result.Images)
            {
                try
                {
                    await _ftpService.DeleteFileAsync($"event-requirements/{eventId}", img.FileName);
                }
                catch
                {
                    // Si el archivo FTP no se puede borrar, continuamos con el borrado en BD.
                }
            }
        }

        var response = await _unitOfWork.ClearFieldAsync(eventId, requirementId);
        if (!response.Success)
            return BadRequest(response.Message);

        return Ok(response.Result);
    }
}
