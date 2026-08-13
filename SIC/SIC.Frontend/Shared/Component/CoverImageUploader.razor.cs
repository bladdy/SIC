using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;

namespace SIC.Frontend.Shared.Component;

public partial class CoverImageUploader : ComponentBase
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    [Parameter] public string? Value { get; set; }

    [Parameter] public EventCallback<string?> ValueChanged { get; set; }

    [Parameter] public string Label { get; set; } = "Miniatura (Foto de Portada)";

    [Inject] private IRepository Repository { get; set; } = default!;

    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

    private byte[]? PendingBytes { get; set; }
    private string? PendingFileName { get; set; }
    private string? PendingPreview { get; set; }
    private bool Uploading { get; set; }

    private bool HasImage => !string.IsNullOrEmpty(PendingPreview) || !string.IsNullOrEmpty(Value);

    public bool HasPendingImage => PendingBytes != null;

    public async Task<string?> UploadPendingImageAsync()
    {
        if (PendingBytes == null)
            return null;

        Uploading = true;
        StateHasChanged();

        try
        {
            using var stream = new MemoryStream(PendingBytes);
            var response = await Repository.UploadFileAsync<object, UploadThumbnailDTO>(
                "api/Events/upload-thumbnail", stream, PendingFileName!);

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync() ?? "No se pudo subir la imagen.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return null;
            }

            var url = response.Response?.Url;
            if (string.IsNullOrWhiteSpace(url))
            {
                await SweetAlertService.FireAsync("Error", "No se pudo subir la imagen.", SweetAlertIcon.Error);
                return null;
            }

            ClearPending();
            await ValueChanged.InvokeAsync(url);
            return url;
        }
        catch
        {
            await SweetAlertService.FireAsync("Error", "No se pudo subir la imagen.", SweetAlertIcon.Error);
            return null;
        }
        finally
        {
            Uploading = false;
            StateHasChanged();
        }
    }

    private async Task OnTextChanged(ChangeEventArgs e)
    {
        ClearPending();
        await ValueChanged.InvokeAsync(e.Value?.ToString());
    }

    private async Task Clear()
    {
        ClearPending();
        await ValueChanged.InvokeAsync(null);
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null || file.Size == 0)
            return;

        if (!file.ContentType.StartsWith("image/"))
        {
            await SweetAlertService.FireAsync("Error", "Solo se permiten archivos de imagen.", SweetAlertIcon.Error);
            return;
        }

        if (file.Size > MaxFileSize)
        {
            await SweetAlertService.FireAsync("Error", "La imagen supera el tamaño máximo de 10 MB.", SweetAlertIcon.Error);
            return;
        }

        await using var stream = file.OpenReadStream(MaxFileSize);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        PendingBytes = memoryStream.ToArray();
        PendingFileName = file.Name;
        PendingPreview = $"data:{file.ContentType};base64,{Convert.ToBase64String(PendingBytes)}";

        StateHasChanged();
    }

    private void ClearPending()
    {
        PendingBytes = null;
        PendingFileName = null;
        PendingPreview = null;
    }
}
