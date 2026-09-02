using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Frontend.Resources;
using SIC.Frontend.Services;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.Album
{
    public partial class AlbumIndex
    {
        //ToDo: boton GenerarQR para cada evento
        [Parameter] public string? Code { get; set; }

        [Inject] private PageMetaService PageMetaService { get; set; } = default!;
        [Inject] private IStringLocalizer<SharedResource> Localizer { get; set; } = default!;
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
        public List<EventImage>? EventImages { get; set; } = [];
        public Event? Event { get; set; }
        private EventImage? PreviewImage;
        private string ActiveTab = "foto";
        private bool IsLoadingBanner = false;
        private bool IsCopyURl = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadEvent();
            if (Event is not null && Event.HasAlbum)
            {
                await LoadEventImage(); 
                if (Event is not null && !string.IsNullOrEmpty(Event.CoverImageUrl))
                {
                    PageMetaService.Set(
                        title: Event.Name,
                        description: Event.SubTitle,
                        image: Event.CoverAlbumImageUrl,
                        canonicalUrl: $"https://invboxv-app.com/my-album/{Event.Code}",
                        keywords: "",
                        ogType: "website"
                    );
                }
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
                Title = "�Est� seguro?",
                Text = $"Se eliminara esta foto. Esta acci�n no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "S�, borrar",
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
                await toast.FireAsync("Error", "Algo salio mal, intentalo de nuevo m�s tarde.", SweetAlertIcon.Error);
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
                await toast.FireAsync("Error", "Algo salio mal, intentalo de nuevo m�s tarde.", SweetAlertIcon.Error);
            }
        }

        private async Task DownloadAllTexts(string code)
        {
            try
            {
                IsLoadingBanner = true;
                var url = $"api/images/DownloadAllTexts/{code}";
                var bytes = await Repository.GetFileAsync(url);
                if (bytes == null || bytes.Length == 0)
                {
                    await SweetAlertService.FireAsync(
                        "Error",
                        "No se pudo generar el banner.",
                        SweetAlertIcon.Error
                    );
                    return;
                }
                await JsRuntime.DownloadFileAsync(
                    $"Texts-Event-{Event?.Name}.pdf",
                    bytes,
                    "application/pdf"
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
            finally
            {
                IsLoadingBanner = false;
            }
        }

        private async Task CopiarEventUrl()
        {
            IsCopyURl = true;
            var url = $"{NavigationManager.BaseUri}upload-photo/{Code}";

            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", url);
            await Task.Delay(1500);
            IsCopyURl = false;
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

        private async Task GenerateBanner(string code)
        {
            try
            {
                IsLoadingBanner = true;
                var url = $"api/images/bannerpdf/{code}";
                var bytes = await Repository.GetFileAsync(url);
                if (bytes == null || bytes.Length == 0)
                {
                    await SweetAlertService.FireAsync(
                        "Error",
                        "No se pudo generar el banner.",
                        SweetAlertIcon.Error
                    );
                    return;
                }
                await JsRuntime.DownloadFileAsync(
                    $"Banner-Event-{Event?.Name}.pdf",
                    bytes,
                    "application/pdf"
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
            finally
            {
                IsLoadingBanner = false;
            }
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
    }
}