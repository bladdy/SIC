//@page "/my-events/details/{Code}"
using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.MyEvents;

//Todo: Agregar disable cuando se Crea o se actualiza una invitacion

[Authorize(Roles = "Admin")]
public partial class MyEventsDetails
{
    // Estados dinámicos por ID
    private int? loadingWhatsappId;

    private int? copyingId;
    private string copyButtonText = "Copiar Invitación";
    private bool usarWhatsApp = true;
    private int currentPage = 1;
    private int totalPages;
    private bool isLoading = false;
    private Invitation NewInvitation = new();
    private bool IsModalVisible = false;
    private bool IsEditMode = false;

    private bool isLoadingImport = false;
    private string? importResult;

    private bool hasFileSelected = false;
    private IBrowserFile? selectedFile;
    private DateTime MinAllowedDate { get; set; } = new DateTime(2023, 1, 1); // Sets January 1, 2023 as the minimum
    public Event? EventDetail { get; set; }
    public List<Invitation>? Invitations { get; set; }
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public int RecordsNumber { get; set; } = 10;

    [Parameter] public string? Code { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadEvent();
        await LoadInvitations();
    }

    private async Task SelectedPageAsync(int page)
    {
        currentPage = page;
        await LoadInvitations(currentPage);
    }

    private async Task DescargarExcel()
    {
        try
        {
            isLoading = true;
            var content = await Repository.GetFileAsync($"api/excel/GenerarExcel/{EventDetail!.Id}");

            if (content.Length > 0)
            {
                await JsRuntime.DownloadFileAsync("reporte.xlsx", content);
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task AbrirWhatsapp(string phoneNumber, string code, int invitationId)
    {
        loadingWhatsappId = invitationId;

        var mensaje = $"Hola, te comparto tu invitación con el código: {code}";
        var url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(mensaje)}";

        await JsRuntime.InvokeVoidAsync("window.open", url, "_blank");

        await Task.Delay(1000); // pequeña pausa solo visual
        loadingWhatsappId = null;
    }

    private async Task CopiarInvitacion(string code, int invitationId)
    {
        copyingId = invitationId;
        copyButtonText = "Copiando mensaje...";

        var mensaje = $"Hola, te comparto tu invitación con el código: {code}";
        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", mensaje);

        await Task.Delay(1000); // Mostrar el loading
        copyButtonText = "Mensaje copiado";

        await Task.Delay(1500); // Mostrar el mensaje copiado un momento
        copyButtonText = "Copiar Invitación";
        copyingId = null;
    }

    private void ShowCreateModal()
    {
        NewInvitation = new Invitation();
        NewInvitation.EventId = EventDetail!.Id;
        IsEditMode = false;
        IsModalVisible = true;
    }

    private void ShowEditModal(Invitation invitation)
    {
        NewInvitation = new Invitation
        {
            Id = invitation.Id,
            Code = invitation.Code,
            Email = invitation.Email,
            EventId = invitation.EventId,
            PhoneNumber = invitation.PhoneNumber,
            NumberAdults = invitation.NumberAdults,
            NumberChildren = invitation.NumberChildren,
            NumberConfirmedAdults = invitation.NumberConfirmedAdults,
            NumberConfirmedChildren = invitation.NumberConfirmedChildren,
            Table = invitation.Table,
            Comments = invitation.Comments,
            SentDate = invitation.SentDate,
            ConfirmationDate = invitation.ConfirmationDate,
            Name = invitation.Name,
            Status = invitation.Status
        };
        IsEditMode = true;
        IsModalVisible = true;
    }

    private void CloseModal()
    {
        IsModalVisible = false;
    }

    private async Task SaveInvitation()
    {
        HttpResponseWrapper<object>? responseHttp;

        if (IsEditMode)
        {
            // PUT -> Editar
            responseHttp = await Repository.PutAsync("api/Invitations/full", NewInvitation);
        }
        else
        {
            // POST -> Crear
            responseHttp = await Repository.PostAsync("api/Invitations/full", NewInvitation);
        }

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el Invitacion.";
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        CloseModal();

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
            "Éxito",
            IsEditMode ? "Invitacion actualizada con éxito." : "Invitacion creada con éxito.",
            SweetAlertIcon.Success
        );

        await LoadInvitations();
    }

    private void HandleFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
        hasFileSelected = selectedFile != null;
    }

    private async Task SubirExcel()
    {
        if (selectedFile == null) return;
        HttpResponseWrapper<ImportExcelResultDTO>? responseHttp;

        try
        {
            isLoadingImport = true;
            importResult = null;

            using var content = new MultipartFormDataContent();
            using var stream = selectedFile.OpenReadStream(5_000_000); // 5MB max
            content.Add(new StreamContent(stream), "file", selectedFile.Name);
            if (content == null)
            {
                await SweetAlertService.FireAsync("Error", "Debes de seleccionar un archivo Excel", SweetAlertIcon.Error);
                return;
            }
            else
            {
                responseHttp = await Repository.UploadFileAsync<object, ImportExcelResultDTO>(
                        $"api/excel/ImportarExcel/{EventDetail!.Id}",
                        stream,
                        selectedFile.Name
                    );
                if (!responseHttp.Error)
                {
                    var result = responseHttp.Response;
                    importResult = $"? Archivo procesado: {result}";
                    await SweetAlertService.FireAsync("Invitaciones",
                            $"Agregadas: {result!.Agregadas}\n" +
                            $"Modificadas: {result!.Modificadas}\n" +
                            $"Errores: {result!.Errores}\n" +
                            $"Total procesadas: {result!.Total}\n\n" +
                            "? Las invitaciones se actualizaron correctamente",
                            SweetAlertIcon.Info);

                    await LoadInvitations();
                }
                else
                {
                    var error = responseHttp.Error;
                    importResult = $"? Error: {error}";
                }
            }
        }
        finally
        {
            isLoadingImport = false;
        }
    }

    private async Task LoadInvitations(int page = 1)
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

    private async Task CleanFilterAsync()
    {
        Filter = string.Empty;
        await ApplyFilterAsync();
    }

    private async Task ApplyFilterAsync()
    {
        int page = 1;
        await LoadInvitations(page);
        await SelectedPageAsync(page);
    }

    private async Task<bool> LoadListAsync(int page)
    {
        var url = $"api/Invitations/paginated?Id={EventDetail!.Id}&PageNumber={page}&RecordsNumber={RecordsNumber}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }

        var responseHttp = await Repository.GetAsync<List<Invitation>>(url);

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

        Invitations = responseHttp?.Response ?? new List<Invitation>();
        return true;
    }

    private async Task LoadPagesAsync()
    {
        var url = $"api/Invitations/totalRecords?Id={EventDetail!.Id}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }
        var responseHttp = await Repository.GetAsync<int>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        // Backend ya devuelve total de páginas, no de registros
        totalPages = responseHttp.Response;
    }

    private async Task LoadEvent()
    {
        var responseHttp = await Repository.GetAsync<Event>($"api/Events/byCode/{Code}");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo("/events");
                return;
            }
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        EventDetail = responseHttp?.Response;
    }

    private async Task FilterCallBack(string filter)
    {
        Filter = filter;
        await ApplyFilterAsync();
        StateHasChanged();

        Filter = filter;
    }
}