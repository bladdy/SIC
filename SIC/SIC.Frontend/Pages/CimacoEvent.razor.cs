using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using SIC.Frontend.Helpers;
using SIC.Frontend.Pages.Events;
using SIC.Frontend.Repositories;
using SIC.Frontend.Shared.Component;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Net;
using static System.Net.WebRequestMethods;

namespace SIC.Frontend.Pages
{
    public partial class CimacoEvent
    {
        [Parameter] public string Code { get; set; } = null!;
        [Parameter] public string? Qr { get; set; }

        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        // Form Generar QR
        private int CantidadQr { get; set; } = 1;

        private bool isLoading;

        // Form Asignar QR
        private string CodigoQr { get; set; } = string.Empty;

        private bool isLoadingImport = false;
        public PhotoEvent? EventDetail { get; set; }
        private PhotoEventImage? PreviewImage;
        private List<PhotoEventImage> EventImages = new();
        private IReadOnlyList<IBrowserFile>? Imagenes;

        private InputFile? inputFileRef;

        private bool IsUploadDisabled =>
            isLoadingImport ||
            string.IsNullOrWhiteSpace(CodigoQr) ||
            Imagenes == null ||
            !Imagenes.Any();

        protected override async Task OnInitializedAsync()
        {
            await LoadEvent();
        }
        private async Task ConfirmDelete(EventImage eventImage)
        {
            ClosePreview();
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "¿Está seguro?",
                Text = $"Se eliminara esta foto. Esta acción no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, borrar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                await DeleteEventImage(eventImage);
            }

            PreviewImage = null;
        }

        private async Task DeleteEventImage(EventImage eventImage)
        {
            var responseHttp = await Repository.DeleteAsync<Event>($"api/images/{Code}/{eventImage.FileName}/{eventImage.Id}");

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar la foto.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync("Eliminada", "La foto fue eliminada correctamente.", SweetAlertIcon.Success);

            await LoadEvent();
            StateHasChanged();
        }
        private async Task DownloadAllImages(string Code)
        {
            try
            {//https://localhost:7174/photo-event/V76NBN/789999
                var content = await Repository.GetFileAsync($"api/PhotoEvent/download-all/{Code}");
                if (content.Length > 0)
                {
                    await JsRuntime.DownloadFileAsync($"Evento-{Code}.zip", content);
                }
            }
            catch (Exception)
            {
                var toast = SweetAlertService.Mixin(new SweetAlertOptions
                {
                    Toast = true,
                    Position = SweetAlertPosition.TopEnd,
                    ShowConfirmButton = false,
                    Timer = 3000,
                    TimerProgressBar = true,
                });
                await toast.FireAsync("Error", "Algo salio mal, intentalo de nuevo más tarde.", SweetAlertIcon.Error);
            }
        }

        private async Task LoadEvent()
        {
            var responseHttp = await Repository.GetAsync<PhotoEvent>($"api/PhotoEvent/{Code}");
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
            EventDetail = responseHttp?.Response;
            if (!string.IsNullOrEmpty(Qr))
            {
                EventImages = EventDetail!.Images.Where(x => x.Code == Qr).ToList();
            }
            else
            {
                EventImages = (List<PhotoEventImage>)EventDetail!.Images;
            }
        }

        private async Task GenerarQr()
        {
            // 1️⃣ Validar cantidad
            if (CantidadQr <= 0)
                return;

            // 2️⃣ Construir URL del endpoint
            var url =
                $"api/PhotoEvent/qr" +
                $"?cantidad={CantidadQr}" +
                $"&evento={Uri.EscapeDataString(Code)}";

            try
            {
                isLoading = true;
                var content = await Repository.GetFileAsync(url);

                if (content.Length > 0)
                {
                    await JsRuntime.DownloadFileAsync($"{Code}.pdf", content);
                }
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OpenPreview(PhotoEventImage eventImage)
        {
            PreviewImage = eventImage;
        }

        private void ClosePreview()
        {
            PreviewImage = null;
        }

        public class QrResponse
        {
            public string Qr { get; set; } = null!;
        }

        private void OnFilesSelected(InputFileChangeEventArgs e)
        {
            Imagenes = e.GetMultipleFiles().ToList();
        }

        private async Task SubirFotos()
        {
            if (Imagenes == null || !Imagenes.Any())
            {
                await SweetAlertService.FireAsync(
                    "Error",
                    "Debes seleccionar al menos una imagen.",
                    SweetAlertIcon.Error
                );
                return;
            }

            try
            {
                isLoadingImport = true;

                using var content = new MultipartFormDataContent();

                foreach (var file in Imagenes)
                {
                    // 🔹 Validación extra (por si acaso)
                    if (file.Size > 10 * 1024 * 1024)
                    {
                        await SweetAlertService.FireAsync(
                            "Archivo muy grande",
                            $"La imagen \"{file.Name}\" supera el límite de 10 MB.",
                            SweetAlertIcon.Warning
                        );
                        return;
                    }

                    var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);

                    content.Add(
                        new StreamContent(stream),
                        "files",
                        file.Name
                    );
                }

                var responseHttp =
                    await Repository.PostMultipartAsync<List<bool>>(
                        $"api/PhotoEvent/upload/{Code}/{CodigoQr}",
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
                        "Las imágenes fueron subidas correctamente.",
                        SweetAlertIcon.Success
                    );

                    await LoadEvent();

                    // 🧹 LIMPIEZA
                    CodigoQr = string.Empty;
                    Imagenes = null;
                    inputFileRef = null;

                    StateHasChanged();
                }
                else
                {
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync(
                        "Error",
                        message ?? "Error al subir las imágenes.",
                        SweetAlertIcon.Error
                    );
                }
            }
            catch (IOException)
            {
                // 🔴 Excede maxAllowedSize
                await SweetAlertService.FireAsync(
                    "Archivo demasiado grande",
                    "Una o más imágenes superan el límite permitido de 10 MB.",
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

        private async Task DownloadImage(PhotoEventImage eventImage)
        {
            try
            {
                var content = await Repository.GetFileAsync($"api/images/download/{Code}/{eventImage.FileName}");
                if (content.Length > 0)
                {
                    await JsRuntime.DownloadFileAsync(eventImage.FileName!, content);
                }
            }
            catch (Exception)
            {
                var toast = SweetAlertService.Mixin(new SweetAlertOptions
                {
                    Toast = true,
                    Position = SweetAlertPosition.TopEnd,
                    ShowConfirmButton = false,
                    Timer = 3000,
                    TimerProgressBar = true,
                });
                await toast.FireAsync("Error", "Algo salio mal, intentalo de nuevo más tarde.", SweetAlertIcon.Error);
            }
        }
    }
}