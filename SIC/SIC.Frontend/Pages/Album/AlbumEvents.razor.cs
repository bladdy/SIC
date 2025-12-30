using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;
using Microsoft.AspNetCore.Components.Forms;

namespace SIC.Frontend.Pages.Album
{
    [Authorize(Roles = "Admin")]
    public partial class AlbumEvents
    {
        [Inject] private IRepository repository { get; set; } = default!;

        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;

        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public DateTime? DateSelectd { get; set; } = null;
        [Parameter, SupplyParameterFromQuery] public int? SelectedEventType { get; set; }
        [Parameter, SupplyParameterFromQuery] public string OrderBy { get; set; } = "";
        [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }

        private List<Event>? Events;
        private List<EventType>? EventTypes;
        private Event NewEvent = new();
        private bool IsModalVisible = false;
        private bool IsEditMode = false;

        private string? importResult;

        private bool isLoadingImport = false;
        private IBrowserFile? selectedFile;
        private bool hasFileSelected = false;

        private int currentPage = 1;
        private int totalPages;

        private string filterText = string.Empty;
        private bool isPreselectedUser = false;

        protected override async Task OnInitializedAsync()
        {
            RecordsNumber ??= 15;
            await LoadEventTypes();
            await LoadEvents(currentPage);
        }

        private async Task LoadEventTypes()
        {
            var response = await repository.GetAsync<List<EventType>>("api/EventTypes");
            if (!response.Error)
                EventTypes = response.Response;
        }

        private async Task LoadEvents(int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(Page))
            {
                page = Convert.ToInt32(Page);
            }
            var ok = await LoadListAsync(page);
            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/Events/totalRecords?PageSize={RecordsNumber}";

            url += $"&HasAlbum={true}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&Filter={Filter}";
            }

            if (DateSelectd != null)
            {
                url += $"&Date={DateSelectd}";
            }
            if (SelectedEventType != null)
            {
                url += $"&EventTypeId={SelectedEventType}";
            }

            var responseHttp = await repository.GetAsync<int>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            totalPages = responseHttp.Response;
        }

        private async Task<bool> LoadListAsync(int page)
        {
            var url = $"api/Events/paginated?PageNumber={page}&PageSize={RecordsNumber}";
            url += $"&HasAlbum={true}";
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                url += $"&Filter={Filter}";
            }
            if (DateSelectd != null)
            {
                url += $"&Date={DateSelectd}";
            }
            if (SelectedEventType != null)
            {
                url += $"&EventTypeId={SelectedEventType}";
            }
            if (!string.IsNullOrWhiteSpace(OrderBy))
            {
                url += $"&OrderBy={OrderBy}";
            }
            var responseHttp = await repository.GetAsync<List<Event>>(url);

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/events");
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return false;
                }
            }

            Events = responseHttp?.Response ?? new List<Event>();
            return true;
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadEvents(page);
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadEvents(page);
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            await LoadEvents(1);
        }

        //  Editar evento existente
        private void ShowEditModal(Event evnt)
        {
            NewEvent = new Event
            {
                Id = evnt.Id,
                Code = evnt.Code,
                CoverImageUrl = evnt.CoverImageUrl
            };

            IsEditMode = true;
            IsModalVisible = true;
        }

        private void HandleFileSelected(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
            hasFileSelected = selectedFile != null;
        }

        private void CloseModal() => IsModalVisible = false;

        private async Task UploadPhoto()
        {
            if (selectedFile == null) return;
            HttpResponseWrapper<EventImage>? responseHttp;
            try
            {
                isLoadingImport = true;
                importResult = null;

                using var content = new MultipartFormDataContent();
                using var stream = selectedFile.OpenReadStream(5_000_000); // 5MB max
                content.Add(new StreamContent(stream), "file", selectedFile.Name);
                if (content == null)
                {
                    await SweetAlertService.FireAsync("Error", "Debes de seleccionar una foto.", SweetAlertIcon.Error);
                }
                else
                {
                    responseHttp = await repository.UploadFileAsync<object, EventImage>(
                            $"api/Events/upload-frontpage/{NewEvent.Code}",
                            stream,
                            selectedFile.Name
                        );
                    if (!responseHttp.Error)
                    {
                        // Luego mostrar la notificación
                        var toast = SweetAlertService.Mixin(new SweetAlertOptions
                        {
                            Toast = true,
                            Position = SweetAlertPosition.TopEnd,
                            ShowConfirmButton = false,
                            Timer = 3000,
                            TimerProgressBar = true,
                        });
                        await toast.FireAsync(
                            "Subir foto", "La foto fue subida con éxito.",
                            SweetAlertIcon.Success
                        );
                        CloseModal();
                    }
                    else
                    {
                        await SweetAlertService.FireAsync("Error", "Ha ocurrido un errror, intentalo más tarde.", SweetAlertIcon.Error);
                    }
                }
            }
            catch (Exception)
            {
                await SweetAlertService.FireAsync("Error", "Ha ocurrido un errror, intentalo más tarde.", SweetAlertIcon.Error);
            }
            finally
            {
                isLoadingImport = false;
            }
        }
    }
}