using FluentFTP;
using System.IO.Compression;
using System.Net;

namespace SIC.Backend.Services;

public class FtpStorageService
{
    private readonly IConfiguration _configuration;

    public FtpStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private FtpClient CreateClient()
    {
        //Cambiar a FtpLocal cuando sea local
        var host = _configuration["Ftp:Host"];
        var username = _configuration["Ftp:Username"];
        var password = _configuration["Ftp:Password"];
        var client = new FtpClient(host)
        {
            Credentials = new System.Net.NetworkCredential(username, password),
            Port = int.Parse(_configuration["Ftp:Port"] ?? "21")
        };
        client.Connect();
        return client;
    }

    public Task CreateFolderAsync(string folder)
    {
        using var client = CreateClient();

        var path = $"/{folder}";
        if (!client.DirectoryExists(path))
        {
            client.CreateDirectory(path);
        }

        return Task.CompletedTask;
    }

    public Task<string> UploadImageAsync(
    Stream fileStream,
    string folder,
    string fileName)
    {
        using var client = CreateClient();

        var directory = $"/{folder}";
        if (!client.DirectoryExists(directory))
        {
            client.CreateDirectory(directory);
        }

        var remotePath = $"{directory}/{fileName}";

        // ✅ UploadStream ES SINCRÓNICO
        client.UploadStream(
            fileStream,
            remotePath,
            FtpRemoteExists.Overwrite,
            true
        );

        var baseUrl = _configuration["Ftp:UrlBase"];
        return Task.FromResult($"{baseUrl}/{folder}/{fileName}");
    }

    public Task DeleteFileAsync(string folder, string fileName)
    {
        using var client = CreateClient();

        folder = folder.Trim('/');

        fileName = Path.GetFileName(fileName); // 🔥 elimina rutas

        var remotePath = $"/{folder}/{fileName}";

        if (client.FileExists(remotePath))
        {
            client.DeleteFile(remotePath);
        }

        return Task.CompletedTask;
    }

    public Task<MemoryStream?> DownloadFileAsync(string folder, string fileName)
    {
        using var client = CreateClient();

        folder = folder.Trim('/');
        fileName = Path.GetFileName(fileName);

        var remotePath = $"/{folder}/{fileName}";

        if (!client.FileExists(remotePath))
            return Task.FromResult<MemoryStream?>(null);

        var tempFile = Path.GetTempFileName();

        // 🔥 Descarga real
        client.DownloadFile(tempFile, remotePath);

        var bytes = File.ReadAllBytes(tempFile);
        File.Delete(tempFile);

        return Task.FromResult<MemoryStream?>(new MemoryStream(bytes));
    }

    // ================== DOWNLOAD ZIP ==================
    public Task<MemoryStream?> DownloadFolderAsZipAsync(string folder)
    {
        using var client = CreateClient();

        folder = folder.Trim('/');
        var remotePath = $"/{folder}";

        if (!client.DirectoryExists(remotePath))
            return Task.FromResult<MemoryStream?>(null);

        var zipStream = new MemoryStream();

        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            var files = client.GetListing(remotePath, FtpListOption.AllFiles);

            foreach (var file in files)
            {
                var entry = zip.CreateEntry(file.Name, CompressionLevel.Fastest);

                using var entryStream = entry.Open();
                client.DownloadStream(entryStream, file.FullName);
            }
        }

        zipStream.Position = 0;
        return Task.FromResult<MemoryStream?>(zipStream);
    }
}