using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private readonly FtpStorageService _ftp;
    private readonly IImageUnitOfWork _imageUnitOfWork;

    //IImagesRepository
    public ImagesController(FtpStorageService ftp, IImageUnitOfWork imageUnitOf)
    {
        _ftp = ftp;
        _imageUnitOfWork = imageUnitOf;
    }

    /*[HttpPost("upload/{folder}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo inválido");

        if (string.IsNullOrWhiteSpace(folder))
            return BadRequest("La carpeta es obligatoria");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using var stream = file.OpenReadStream();
        var url = await _ftp.UploadImageAsync(stream, folder, fileName);
        if (string.IsNullOrWhiteSpace(url))
            return BadRequest("Error al subir la imagen");

        var imagene = new EventImageDTO
        {
            CodeEvent = folder,
            ImageUrl = url,
            FileName = fileName,
        };

        var response = await _imageUnitOfWork.AddFullAsyn(imagene);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }*/

    [HttpPost("upload/{folder}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] List<IFormFile> files,
        [FromRoute] string folder)
    {
        if (files == null || !files.Any())
            return BadRequest("No se enviaron archivos");

        if (string.IsNullOrWhiteSpace(folder))
            return BadRequest("La carpeta es obligatoria");

        var imagesSaved = new List<EventImage>();

        foreach (var file in files)
        {
            if (file.Length == 0)
                continue;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            using var stream = file.OpenReadStream();
            var url = await _ftp.UploadImageAsync(stream, folder, fileName);

            if (string.IsNullOrWhiteSpace(url))
                continue;

            var image = new EventImageDTO
            {
                CodeEvent = folder,
                ImageUrl = url,
                FileName = fileName
            };

            var response = await _imageUnitOfWork.AddFullAsyn(image);

            if (response.Success)
                imagesSaved.Add(response.Result);
        }

        if (!imagesSaved.Any())
            return BadRequest("No se pudo subir ninguna imagen");

        var result = imagesSaved.Select(img => new EventImageDTO
        {
            CodeEvent = img.Event.Code,
            ImageUrl = img.Url,
            FileName = img.FileName
        }).ToList();

        return Ok(result);
    }

    [HttpGet("byEvent/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        var response = await _imageUnitOfWork.GetAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpDelete("{folder}/{fileName}/{id}")]
    public async Task<IActionResult> Delete(string folder, string fileName, string id)
    {
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Folder y fileName son obligatorios");

        await _ftp.DeleteFileAsync(folder, fileName);

        var response = await _imageUnitOfWork.DeleteAsync(Convert.ToInt32(id));
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("download/{folder}/{fileName}")]
    public async Task<IActionResult> DownloadImage(string folder, string fileName)
    {
        var stream = await _ftp.DownloadFileAsync(folder, fileName);

        if (stream == null)
            return NotFound();

        var contentType = "image/jpeg"; // o detectarlo dinámicamente

        return File(stream, contentType, fileName);
    }

    [HttpGet("download-all/{folder}")]
    public async Task<IActionResult> DownloadAllImages(string folder)
    {
        var zipStream = await _ftp.DownloadFolderAsZipAsync(folder);

        if (zipStream == null)
            return NotFound();

        return File(
            zipStream,
            "application/zip",
            $"Album_{folder}.zip"
        );
    }
}