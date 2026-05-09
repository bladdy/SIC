using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.Album;

public partial class AddImagenAlbum
{
    [Parameter] public string? Code { get; set; }
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private IReadOnlyList<IBrowserFile>? selectedFiles;

    private bool isRecording;

    private RecordedAudioDTO? recordedAudio;

    private string? recordedAudioUrl;

    private PeriodicTimer? timer;

    private TimeSpan recordingTime = TimeSpan.Zero;

    private string? importResult;

    private bool hasFileSelected = false;

    private bool isLoadingImport = false;
    public Event? Event { get; set; }
    public List<EventImage>? EventImages { get; set; } = [];
    private EventImage? PreviewImage;

    protected override async Task OnInitializedAsync()
    {
        await LoadEvent();
        if (Event is not null && Event.AlbumPublic)
        {
            await LoadEventImage();
        }
    }

    private async Task LoadEventImage()
    {
        var url = $"api/images/byEvent/{Code}";

        var responseHttp = await Repository.GetAsync<List<EventImage>>(url);

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo("/events");
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            }
        }

        EventImages = responseHttp?.Response ?? new List<EventImage>();
    }

    private async Task LoadEvent()
    {
        var responseHttp = await Repository.GetAsync<Event>($"api/Events/byCode/{Code}");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo("/");
                return;
            }
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        Event = responseHttp?.Response;
        if (Event == null)
        {
            NavigationManager.NavigateTo("/");
        }
        else
        {
            if (!Event.HasAlbum)
            {
                NavigationManager.NavigateTo("/");
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("initPopovers", 5000); // 5 segundos
        }
    }

    private void HandleFileSelected(InputFileChangeEventArgs e)
    {
        selectedFiles = e.GetMultipleFiles();
        hasFileSelected = selectedFiles != null && selectedFiles.Any();
    }

    private async Task UploadPhoto()
    {
        if (selectedFiles == null || !selectedFiles.Any())
        {
            await SweetAlertService.FireAsync(
                "Error",
                "Debes seleccionar al menos un archivo.",
                SweetAlertIcon.Error
            );
            return;
        }

        try
        {
            isLoadingImport = true;

            using var content = new MultipartFormDataContent();

            foreach (var file in selectedFiles)
            {
                // 🔹 Validación extra (por si acaso)
                if (file.Size > 10 * 1024 * 1024)
                {
                    await SweetAlertService.FireAsync(
                        "Archivo muy grande",
                        $"El archivo \"{file.Name}\" supera el límite de 10 MB.",
                        SweetAlertIcon.Warning
                    );
                    return;
                }

                var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                // 🔹 IMPORTANTE

                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType =
                    new System.Net.Http.Headers.MediaTypeHeaderValue(
                        file.ContentType);

                content.Add(
                    streamContent,
                    "files",
                    file.Name
                );
            }

            var responseHttp =
                await Repository.PostMultipartAsync<List<EventImageDTO>>(
                    $"api/images/upload/{Code}",
                    content
                );

            if (!responseHttp.Error)
            {
                var toast = SweetAlertService.Mixin(new SweetAlertOptions
                {
                    Toast = true,
                    Position = SweetAlertPosition.TopEnd,
                    ShowConfirmButton = false,
                    Timer = 3000,
                    TimerProgressBar = true
                });

                await toast.FireAsync(
                    "Subir fotos",
                    "Los archivos fueron subidas correctamente.",
                    SweetAlertIcon.Success
                );

                await LoadEventImage();
                selectedFiles = null;
            }
            else
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync(
                    "Error",
                    message ?? "Error al subir los archivos.",
                    SweetAlertIcon.Error
                );
            }
        }
        catch (IOException)
        {
            // 🔴 Excede maxAllowedSize
            await SweetAlertService.FireAsync(
                "Archivo demasiado grande",
                "Un o más archivos superan el límite permitido de 10 MB.",
                SweetAlertIcon.Warning
            );
        }
        catch (Exception)
        {
            // 🔴 Cualquier otro error
            await SweetAlertService.FireAsync(
                "Error",
                "Ha ocurrido un error inesperado.",
                SweetAlertIcon.Error
            );
        }
        finally
        {
            isLoadingImport = false;
        }
    }

    private async Task UploadRecordedAudio()
    {
        if (recordedAudio is null)
            return;

        try
        {
            isLoadingImport = true;

            using var content =
                new MultipartFormDataContent();

            var bytes =
                Convert.FromBase64String(
                    recordedAudio.Base64Data);

            var byteContent =
                new ByteArrayContent(bytes);

            byteContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    recordedAudio.ContentType);

            content.Add(
                byteContent,
                "files",
                recordedAudio.FileName);

            var response =
                await Repository.PostMultipartAsync<List<EventImageDTO>>(
                    $"api/images/upload/{Code}",
                    content);

            if (!response.Error)
            {
                await SweetAlertService.FireAsync(
                    "Éxito",
                    "Audio subido correctamente",
                    SweetAlertIcon.Success);

                await LoadEventImage();

                // 🔹 Limpiar
                recordedAudio = null;
                recordedAudioUrl = null;
                recordingTime = TimeSpan.Zero;
            }
            else
            {
                await SweetAlertService.FireAsync(
                    "Error",
                    "No se pudo subir el audio",
                    SweetAlertIcon.Error);
            }
        }
        finally
        {
            isLoadingImport = false;
        }
    }

    private async Task StartRecording()
    {
        recordingTime = TimeSpan.Zero;

        await JsRuntime.InvokeVoidAsync(
            "audioRecorder.start");

        isRecording = true;

        timer = new PeriodicTimer(
            TimeSpan.FromSeconds(1));

        _ = Task.Run(async () =>
        {
            while (await timer.WaitForNextTickAsync())
            {
                recordingTime =
                    recordingTime.Add(
                        TimeSpan.FromSeconds(1));

                await InvokeAsync(StateHasChanged);
            }
        });
    }

    private async Task StopRecording()
    {
        timer?.Dispose();

        recordedAudio =
            await JsRuntime.InvokeAsync<RecordedAudioDTO>(
                "audioRecorder.stop");

        isRecording = false;

        if (recordedAudio is null)
            return;

        // 🔹 Crear preview para reproducir
        recordedAudioUrl =
            $"data:{recordedAudio.ContentType};base64,{recordedAudio.Base64Data}";

        StateHasChanged();
    }

    private void DiscardRecording()
    {
        recordedAudio = null;

        recordedAudioUrl = null;

        recordingTime = TimeSpan.Zero;
    }

    private void OpenPreview(EventImage eventImage)
    {
        PreviewImage = eventImage;
    }

    private void ClosePreview()
    {
        PreviewImage = null;
    }
}