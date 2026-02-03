using Azure;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SIC.Backend.Helpers;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.IO.Compression;
using static QRCoder.PayloadGenerator;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoEventController : ControllerBase
    {
        private readonly FtpStorageService _ftp;
        private readonly IPhotoEventUnitOfWork _photoEventUnitOf;
        private readonly BoletaService _boletaService;

        public PhotoEventController(FtpStorageService ftp, IPhotoEventUnitOfWork photoEventUnitOf, BoletaService boletaService)
        {
            _ftp = ftp;
            _photoEventUnitOf = photoEventUnitOf;
            _boletaService = boletaService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddPhotoEvent(
            PhotoEventDTO photoEvent)
        {
            if (photoEvent.File == null || photoEvent.File.Length == 0)
                return BadRequest("Archivo inválido");
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photoEvent.File.FileName)}";
            using var stream = photoEvent.File.OpenReadStream();
            var url = await _ftp.UploadImageAsync(stream, "photoevents", fileName);
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("Error al subir la imagen");
            var newPhotoEvent = new PhotoEvent
            {
                PortadaUrl = url,
                Name = photoEvent.Name,
                Images = new List<PhotoEventImage>()
            };

            var response = await _photoEventUnitOf.AddFullPhotoEvenAsyn(newPhotoEvent);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("upload/{folder}/{qr}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
        [FromForm] List<IFormFile> files,
        [FromRoute] string folder, [FromRoute] string qr)
        {
            if (files == null || !files.Any())
                return BadRequest("Archivo inválido");

            if (string.IsNullOrWhiteSpace(folder))
                return BadRequest("La carpeta es obligatoria");
            var response = await _photoEventUnitOf.GetByIdAsync(folder);
            var imagesSaved = new List<bool>();
            if (response.Result == null)
                return BadRequest("El evento no existe.");
            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

                using var stream = file.OpenReadStream();
                var url = await _ftp.UploadImageAsync(stream, folder, fileName);

                if (string.IsNullOrWhiteSpace(url))
                    continue;
                var image = new PhotoEventImage
                {
                    Url = url,
                    FileName = Path.GetFileName(new Uri(url).LocalPath),
                    Code = qr,
                    PhotoEventId = response.Result.Id
                };
                var savedImage = await _photoEventUnitOf.AddFullImageAsyn(image);
                if (savedImage.Success)
                {
                    imagesSaved.Add(true);
                }
            }
            if (!imagesSaved.Any())
                return BadRequest("No se pudo subir ninguna imagen");

            return Ok(imagesSaved);
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode([FromRoute] string code)
        {
            var response = await _photoEventUnitOf.GetByIdAsync(code);
            if (response.Success)
            {
                return Ok(response.Result);
            }
            return NotFound(response);
        }

        [HttpDelete("{folder}")]
        public async Task<IActionResult> Delete(string folder)
        {
            var response = await _photoEventUnitOf.GetByIdAsync(folder);
            if (!response.Success || response.Result == null)
            {
                return NotFound("El evento no existe.");
            }

            foreach (var image in response.Result.Images)
            {
                if (string.IsNullOrWhiteSpace(image.FileName))
                    continue;
                await _ftp.DeleteFileAsync(folder, image.FileName);
            }

            var response2 = await _photoEventUnitOf.RemoveFullAsyn(response.Result.Id);
            if (response2.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("qr")]
        public async Task<IActionResult> GetQrsPdf(int cantidad, string evento)
        {
            var codigos = new List<string>();

            for (int i = 1; i <= cantidad; i++)
            {
                codigos.Add(CodeGenerator.GenerateCode());
            }

            var pdfBytes = _boletaService.GenerarPdfQrs(evento, codigos);

            return File(
                pdfBytes,
                "application/pdf",
                $"{evento}_QR.pdf"
            );
        }
    }
}