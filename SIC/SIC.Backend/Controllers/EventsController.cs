using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;
using System.Text.Json;

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
    {//chequear un error al crear evento
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

    [HttpPost("requirement-status/{id}/{status}")]
    public async Task<IActionResult> SetRequirementFormStatusAsync(int id, Status status)
    {
        var action = await _eventsUnitOfWork.SetRequirementFormStatusAsync(id, status);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest(action.Message);
    }

    [HttpPost("upload-frontpage/{code}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PutFullAsync(
        IFormFile file, string code,
        [FromForm] string cropData)
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
            var crop = JsonSerializer.Deserialize<CropData>(cropData);
            response.Result.CoverPositionX = crop?.X ?? 0;
            response.Result.CoverPositionY = crop?.Y ?? 0;
            response.Result.CoverZoom = crop?.Width ?? 1;
            response.Result.CoverAlbumImageUrl = url;
            action = await _eventsUnitOfWork.UpdateFullAsync(response.Result);
        }
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpGet("qr/download/{code}")]
    public async Task<IActionResult> DownloadQRCodeAsync(string code)
    {
        try
        {
            var response = await _eventsUnitOfWork.GetByCodeAsync(code);
            var events = response.Result;

            if (events == null)
                return NotFound("Invitación no encontrada.");

            var codigoQr = events.Code ?? $"INV-{events.Id}-{code}";

            var qrUrl = $"https://invboxv-app.com/upload-photo/{codigoQr}";

            // 🔹 Generar QR (PNG bytes)
            byte[] qrBytes = GenerateQRCodePng(qrUrl);

            var fileName = $"QR_{codigoQr}.png";

            return File(
                qrBytes,
                "image/png",
                fileName // 👈 fuerza descarga
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /*
    CREARLO nuevo

    [HttpDelete("{id}")]
    public override async Task<IActionResult> DeleteAsync(int id)
    {
        var response = await _eventsUnitOfWork.DeleteAsync(id);
        if (response.Success)
            return Ok(response.Result);
        return BadRequest(response.Message);
    }*/

    private static byte[] GenerateQRCodePng(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        var pngQrCode = new PngByteQRCode(qrCodeData);
        return pngQrCode.GetGraphic(20);
    }
}