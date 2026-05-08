using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.Album
{
    public partial class AlbumIndex
    {
        //ToDo: boton GenerarQR para cada evento
        [Parameter] public string? Code { get; set; }

        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
        public List<EventImage>? EventImages { get; set; } = [];
        public Event? Event { get; set; }
        private EventImage? PreviewImage;

        protected override async Task OnInitializedAsync()
        {
            await LoadEvent();
            if (Event is not null && Event.HasAlbum)
            {
                await LoadEventImage();
            }
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

        private void OpenPreview(EventImage eventImage)
        {
            PreviewImage = eventImage;
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

            await LoadEventImage();
            StateHasChanged();
        }

        private async Task DownloadImage(EventImage eventImage)
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

        private async Task DownloadAllImages(string Code)
        {
            try
            {
                var content = await Repository.GetFileAsync($"api/images/download-all/{Code}");
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

        private async Task GenerateQR(string code)
        {
            try
            {
                var url = $"api/events/qr/download/{code}";

                var bytes = await Repository.GetFileAsync(url);

                if (bytes == null || bytes.Length == 0)
                {
                    await SweetAlertService.FireAsync(
                        "Error",
                        "No se pudo generar el QR.",
                        SweetAlertIcon.Error
                    );
                    return;
                }

                await JsRuntime.DownloadFileAsync(
                    $"QR-Event-{code}.png",
                    bytes
                );
            }
            catch (Exception ex)
            {
                await SweetAlertService.FireAsync(
                    "Error",
                    ex.Message,
                    SweetAlertIcon.Error
                );
            }
        }

        private void ClosePreview()
        {
            PreviewImage = null;
        }
    }
}