using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequirementImageOptionsController : GenericController<RequirementImageOption>
{
    private readonly IRequirementImageOptionsUnitOfWork _unitOfWork;
    private readonly FtpStorageService _ftpService;

    public RequirementImageOptionsController(
        IGenericUnitOfWork<RequirementImageOption> genericUnitOfWork,
        IRequirementImageOptionsUnitOfWork unitOfWork,
        FtpStorageService ftpService)
        : base(genericUnitOfWork)
    {
        _unitOfWork = unitOfWork;
        _ftpService = ftpService;
    }

    [HttpGet("byRequirement/{requirementId}")]
    public async Task<IActionResult> GetByRequirementIdAsync(int requirementId)
    {
        var response = await _unitOfWork.GetByRequirementIdAsync(requirementId);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }

    [HttpPost("save/{requirementId}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SaveOptionsAsync(int requirementId)
    {
        var optionsJson = Request.Form["options"].FirstOrDefault();
        if (string.IsNullOrEmpty(optionsJson))
            return BadRequest("No se proporcionaron opciones.");

        var options = System.Text.Json.JsonSerializer.Deserialize<List<RequirementImageOptionDTO>>(optionsJson);
        if (options == null)
            return BadRequest("Formato de opciones inválido.");

        var filesByIndex = new Dictionary<int, IFormFile>();
        foreach (var file in Request.Form.Files)
        {
            var match = System.Text.RegularExpressions.Regex.Match(file.Name, @"^option_(\d+)$");
            if (match.Success)
            {
                var idx = int.Parse(match.Groups[1].Value);
                filesByIndex[idx] = file;
            }
        }

        for (int i = 0; i < options.Count; i++)
        {
            if (filesByIndex.TryGetValue(i, out var file))
            {
                var folder = $"requirement-options/{requirementId}";
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                using var stream = file.OpenReadStream();
                var url = await _ftpService.UploadRawImageAsync(stream, folder, fileName);
                options[i].ImageUrl = url;
            }
        }

        var response = await _unitOfWork.SaveOptionsAsync(requirementId, options);
        if (!response.Success)
            return BadRequest(response.Message);
        return Ok(response.Result);
    }
}
