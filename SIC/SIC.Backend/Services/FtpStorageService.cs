using FluentFTP;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO.Compression;

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
        //Cambiar a FtpLocal cuando sea Local
        var host = _configuration["FtpLocal:Host"];
        var username = _configuration["FtpLocal:Username"];
        var password = _configuration["FtpLocal:Password"];
        var client = new FtpClient(host)
        {
            Credentials = new System.Net.NetworkCredential(username, password),
            Port = int.Parse(_configuration["FtpLocal:Port"] ?? "21")
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

    public async Task<string> UploadImageAsync(
        Stream fileStream,
        string folder,
        string fileName)
    {
        const int maxWidth = 1280;

        using var client = CreateClient();

        var directory = $"/{folder}";

        EnsureDirectory(client, directory);

        var webpFileName = Path.ChangeExtension(fileName, ".webp");

        var remotePath = $"{directory}/{webpFileName}";

        using var image = await Image.LoadAsync(fileStream);

        if (image.Width > maxWidth)
        {
            image.Mutate(x =>
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, 0)
                }));
        }

        using var webpStream = new MemoryStream();

        await image.SaveAsync(
            webpStream,
            new WebpEncoder
            {
                Quality = 65,
                Method = WebpEncodingMethod.BestQuality
            });

        webpStream.Position = 0;

        client.UploadStream(
            webpStream,
            remotePath,
            FtpRemoteExists.Overwrite,
            true);

        return BuildFileUrl(folder, webpFileName);
    }

    public async Task<string> UploadVideoAsync(
        Stream fileStream,
        string folder,
        string fileName)
    {
        using var client = CreateClient();

        var directory = $"/{folder}";

        EnsureDirectory(client, directory);

        var remotePath = $"{directory}/{fileName}";

        await UploadFileStreamAsync(
            client,
            fileStream,
            remotePath);

        return BuildFileUrl(
            folder,
            fileName);
    }

    public async Task<string> UploadAudioAsync(
        Stream fileStream,
        string folder,
        string fileName)
    {
        using var client = CreateClient();

        var directory = $"/{folder}";

        EnsureDirectory(client, directory);

        var remotePath = $"{directory}/{fileName}";

        await UploadFileStreamAsync(
            client,
            fileStream,
            remotePath);

        return BuildFileUrl(
            folder,
            fileName);
    }

    private Task UploadFileStreamAsync(
        FtpClient client,
        Stream stream,
        string remotePath)
    {
        stream.Position = 0;

        client.UploadStream(
            stream,
            remotePath,
            FtpRemoteExists.Overwrite,
            true
        );
        return Task.CompletedTask;
    }

    private void EnsureDirectory(
        FtpClient client,
        string directory)
    {
        if (!client.DirectoryExists(directory))
        {
            client.CreateDirectory(directory);
        }
    }

    private string BuildFileUrl(
        string folder,
        string fileName)
    {
        var baseUrl = _configuration["FtpLocal:UrlBase"];

        return $"{baseUrl}/{folder}/{fileName}";
    }

    //ToDo: Crear un metodo que diga si o no se borro el archivo
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
        try
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error al descargar carpeta FTP: {ex.Message}");
            return Task.FromResult<MemoryStream?>(null);
        }
    }
}