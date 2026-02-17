using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Implemetations;
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
    private readonly IEventsUnitOfWork _eventsUnitOfWork;

    //IImagesRepository
    public ImagesController(FtpStorageService ftp, IImageUnitOfWork imageUnitOf, IEventsUnitOfWork eventsUnitOfWork)
    {
        _ftp = ftp;
        _imageUnitOfWork = imageUnitOf;
        _eventsUnitOfWork = eventsUnitOfWork;
    }

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
                FileName = Path.GetFileName(new Uri(url).LocalPath)
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

    [HttpDelete("Album/{folder}")]
    public async Task<IActionResult> DeleteAlbum(string folder)
    {
        var response = await _imageUnitOfWork.GetAsync(folder);
        if (response.Success && response.Result != null)
        {
            foreach (var image in response.Result)
            {
                if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(image.FileName))
                    return BadRequest("Folder y fileName son obligatorios");
                await _ftp.DeleteFileAsync(folder, image.FileName);
                var responseDelete = await _imageUnitOfWork.DeleteAsync(Convert.ToInt32(image.Id));
            }
        }
        var responseEvent = await _eventsUnitOfWork.GetByCodeAsync(folder);
        if (!string.IsNullOrEmpty(responseEvent.Result?.CoverAlbumImageUrl))
        {
            Uri uri = new(responseEvent.Result?.CoverAlbumImageUrl ?? string.Empty);
            string fileName = Path.GetFileName(uri.LocalPath);
            await _ftp.DeleteFileAsync("FrontPages", fileName);
        }
        var eventup = responseEvent.Result;
        if (eventup != null)
        {
            eventup.CoverAlbumImageUrl = null;
            eventup.CoverPositionX = 0;
            eventup.CoverPositionY = 0;
            eventup.CoverZoom = 1;
            eventup.AlbumPublic = false;
            eventup.HasAlbum = false;
            await _eventsUnitOfWork.UpdateFullAsync(eventup);
        }
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