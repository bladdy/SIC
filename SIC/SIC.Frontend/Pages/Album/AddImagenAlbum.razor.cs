using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Frontend.Resources;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.Album;

public partial class AddImagenAlbum
{
    [Parameter] public string? Code { get; set; }
    [Inject] private IStringLocalizer<SharedResource> Localizer { get; set; } = default!;
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private IReadOnlyList<IBrowserFile>? selectedFiles;
    private string ActiveTab = "foto";

    private bool isRecording;
    private bool isWriting;

    private RecordedAudioDTO? recordedAudio;

    private string? recordedAudioUrl;

    private PeriodicTimer? timer;

    private TimeSpan recordingTime = TimeSpan.Zero;

    private string? importResult;

    private bool hasFileSelected = false;

    private bool isLoadingMessage = false;
    private bool isLoadingImport = false;
    private EventImageDTO eventImageDTO = new();
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
            hasFileSelected = false;
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

    private void ChangeTab(string tab)
    {
        ActiveTab = tab;
    }

    private string GetTabStyle(string tab)
    {
        bool isActive = ActiveTab == tab;

        return isActive
            ? "height:68px;border-radius:14px;border:1px solid #3C6A79;background:#3C6A79;color:#ffffff;padding:6px 4px;box-shadow:0 2px 8px rgba(0,0,0,.12);"
            : "height:68px;border-radius:14px;border:1px solid #ebe7e2;background:#f8f6f3;color:#9b7b45;padding:6px 4px;";
    }

    private void ClosePreview()
    {
        PreviewImage = null;
    }

    private async Task PublishMessage()
    {
        try
        {
            isLoadingMessage = true;
            var responseHttp =
                await Repository.PostAsync<EventImageDTO>(
                    $"api/images/full/{Code}", eventImageDTO
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
                    "Enviar dedicatoria",
                    "Tu dedicatoria fue enviada correctamente.",
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
                    message ?? "Error al enviar tu dedicatoria.",
                    SweetAlertIcon.Error
                );
            }
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
            isLoadingMessage = false;
            WriteMessage();
        }
    }

    private void WriteMessage()
    {
        isWriting = !isWriting;
        eventImageDTO = new();
    }
}