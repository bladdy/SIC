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

    [HttpPost("full/{Code}")]
    public async Task<IActionResult> PostFullAsync(string Code, EventImageDTO eventImage)
    {
        eventImage.CodeEvent = Code;
        eventImage.ImageType = "text";
        eventImage.ImageUrl = "ESTO NO ES UNA IMAGEN";
        var action = await _imageUnitOfWork.AddFullAsyn(eventImage);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
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

            try
            {
                EventImage? result = null;

                if (file.ContentType.StartsWith("image/"))
                {
                    result = await UploadImageAsync(file, folder);
                }
                else if (file.ContentType.StartsWith("video/"))
                {
                    result = await UploadVideoAsync(file, folder);
                }
                else if (file.ContentType.StartsWith("audio/"))
                {
                    result = await UploadAudioAsync(file, folder);
                }

                if (result != null)
                    imagesSaved.Add(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        if (!imagesSaved.Any())
            return BadRequest("No se pudo subir ningún archivo");

        var resultResponse = imagesSaved.Select(img => new EventImageDTO
        {
            CodeEvent = img.Event.Code,
            ImageUrl = img.Url,
            FileName = img.FileName
        }).ToList();

        return Ok(resultResponse);
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

    private async Task<EventImage?> UploadImageAsync(
    IFormFile file,
    string folder)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using var stream = file.OpenReadStream();

        var url = await _ftp.UploadImageAsync(
            stream,
            folder,
            fileName);

        if (string.IsNullOrWhiteSpace(url))
            return null;

        return await SaveFileRecordAsync(folder, url, "imagen");
    }

    private async Task<EventImage?> UploadVideoAsync(
        IFormFile file,
        string folder)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using var stream = file.OpenReadStream();

        var url = await _ftp.UploadVideoAsync(
            stream,
            folder,
            fileName);

        if (string.IsNullOrWhiteSpace(url))
            return null;

        return await SaveFileRecordAsync(folder, url, "video");
    }

    private async Task<EventImage?> UploadAudioAsync(
        IFormFile file,
        string folder)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

        using var stream = file.OpenReadStream();

        var url = await _ftp.UploadAudioAsync(
            stream,
            folder,
            fileName);

        if (string.IsNullOrWhiteSpace(url))
            return null;

        return await SaveFileRecordAsync(folder, url, "audio");
    }

    private async Task<EventImage?> SaveFileRecordAsync(
        string folder,
        string url, string ImageType)
    {
        var dto = new EventImageDTO
        {
            CodeEvent = folder,
            ImageUrl = url,
            FileName = Path.GetFileName(new Uri(url).LocalPath),
            ImageType = ImageType
        };

        var response = await _imageUnitOfWork.AddFullAsyn(dto);

        if (!response.Success)
            return null;

        return response.Result;
    }
}