using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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
    private string? importResult;

    private IReadOnlyList<IBrowserFile>? selectedFiles;
    private bool hasFileSelected = false;

    private bool isLoadingImport = false;
    public Event? Event { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadEvent();
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
                "Debes seleccionar al menos una imagen.",
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
                var stream = file.OpenReadStream(5_000_000); // 5MB por imagen

                content.Add(
                    new StreamContent(stream),
                    "files",        // 👈 DEBE coincidir con el backend
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
                    "Las imágenes fueron subidas correctamente.",
                    SweetAlertIcon.Success
                );

                await LoadEvent();
                selectedFiles = null;
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
        catch
        {
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
}