using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.RegisterEvent;

public partial class InvitationsEntriesIndex
{
    private int currentPage = 1;
    private int totalPages;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Parameter] public string? Code { get; set; }
    [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public int? RecordsNumber { get; set; }
    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public string OrderBy { get; set; } = "";
    [Inject] private IRepository Repository { get; set; } = default!;
    public List<InvitationEntry>? InvitationEntries { get; set; }
    private InvitationEntry NewInvitationEntry = new();
    private InvitationEntry EditedInvitationEntry = new();
    private Invitation? Invitation = null;
    private bool IsModalVisible = false;
    private bool IsModalEditVisible = false;
    private bool IsEditMode = false;

    private DotNetObjectReference<object>? objRef;
    private bool isScannerRunning = false;
    private string? qrResult;
    private bool isGeneratingPdf = false;

    protected override async Task OnInitializedAsync()
    {
        RecordsNumber ??= 15;
        if (!string.IsNullOrWhiteSpace(Page) && int.TryParse(Page, out var pageFromQuery))
        {
            currentPage = pageFromQuery;
        }
        await base.OnInitializedAsync();
        await LoadInvitationEntries(currentPage);
    }

    private async Task LoadInvitationEntries(int page = 1)
    {
        var ok = await LoadListAsync(page);
        if (ok)
        {
            await LoadPagesAsync();
        }
    }

    private async Task<bool> LoadPagesAsync()
    {
        var url = $"api/InvitationEntry/paginated?Code={Code}&RecordsNumber={RecordsNumber ?? 15}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }

        if (!string.IsNullOrWhiteSpace(OrderBy))
        {
            url += $"&OrderBy={OrderBy}";
        }
        var responseHttp = await Repository.GetAsync<List<InvitationEntry>>(url);

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

        InvitationEntries = responseHttp?.Response;
        return true;
    }

    private async Task<bool> LoadListAsync(int page)
    {
        var url = $"api/InvitationEntry/paginated?Code={Code}&PageNumber={page}&PageSize={RecordsNumber ?? 15}";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }

        if (!string.IsNullOrWhiteSpace(OrderBy))
        {
            url += $"&OrderBy={OrderBy}";
        }
        var responseHttp = await Repository.GetAsync<List<InvitationEntry>>(url);

        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo($"/events/details/{Code}");
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
        }

        InvitationEntries = responseHttp?.Response ?? new List<InvitationEntry>();
        return true;
    }

    private async Task ApplyFilterAsync()
    {
        int page = 1;
        await LoadInvitationEntries(page);
        await SelectedPageAsync(page);
    }

    private async Task SelectedPageAsync(int page)
    {
        currentPage = page;
        await LoadInvitationEntries(currentPage);
    }

    private async Task CleanFilterAsync()
    {
        Filter = string.Empty;
        await ApplyFilterAsync();
    }

    private async Task GeneratePdfAsync()
    {
        isGeneratingPdf = true;
        try
        {
            var content = await Repository.GetFileAsync($"api/InvitationEntry/generatedpdf?evento={Code}");
            if (content != null && content.Length > 0)
            {
                await JS.DownloadFileAsync($"registro-invitados-{Code}.pdf", content, "application/pdf");
            }
            else
            {
                await SweetAlertService.FireAsync("Error", "No hay invitados registrados para generar el PDF.", SweetAlertIcon.Error);
            }
        }
        finally
        {
            isGeneratingPdf = false;
        }
    }

    private void ShowEditModal(InvitationEntry invitationEntry)
    {
        IsEditMode = false;
        IsModalEditVisible = true;
        EditedInvitationEntry = invitationEntry;
    }

    private async Task ShowCreateModal()
    {
        IsEditMode = false;
        IsModalVisible = true;
        Invitation = null;
        await StartScannerAsync();
    }

    private void CloseEditModal()
    {
        IsModalEditVisible = false;
    }

    private async void CloseModal()
    {
        IsModalVisible = false;
        StateHasChanged();
        await OnQrCodeScannedClose();
    }

    private async Task StartScannerAsync()
    {
        if (isScannerRunning) return;

        objRef?.Dispose(); // limpiar referencia anterior
        objRef = DotNetObjectReference.Create<object>(this);

        await Task.Delay(200);

        await JS.InvokeVoidAsync("qrScanner.start", objRef);
        isScannerRunning = true;
    }

    [JSInvokable]
    public async Task OnQrCodeScannedClose()
    {
        await JS.InvokeVoidAsync("qrScanner.stop");
        isScannerRunning = false;
        StateHasChanged();
        //Cargar el valor escaneado en el campo correspondiente
    }

    [JSInvokable]
    public async Task OnQrCodeScanned(string code)
    {
        qrResult = code;
        //Cargar el valor escaneado en el campo correspondiente
        await LoadInvitation(qrResult);

        await JS.InvokeVoidAsync("qrScanner.stop");
        isScannerRunning = false;
        StateHasChanged();
    }

    private async Task LoadInvitation(string qrResult)
    {
        Invitation = null;
        var responseHttp = await Repository.GetAsync<Invitation>($"api/Invitations/byCode/{qrResult}");
        if (responseHttp.Response == null)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                await SweetAlertService.FireAsync("Info", "La invitacion no existe.", SweetAlertIcon.Error);
                StateHasChanged();
                await OnQrCodeScannedClose();
                await Task.Delay(200);
                await StartScannerAsync();
                return;
            }
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        Invitation = responseHttp?.Response!;
        NewInvitationEntry = Invitation?.InvitationEntry ?? new InvitationEntry();
    }

    private async Task UpdateInvitationEntry()
    {
        InvitationEntry invitationEntry = new()
        {
            Id = EditedInvitationEntry.Id,
            Code = EditedInvitationEntry.Code,
            InvitationId = EditedInvitationEntry.InvitationId,
            EventId = EditedInvitationEntry.EventId,
            AdultsEntered = EditedInvitationEntry.AdultsEntered,
            YouthsEntered = EditedInvitationEntry.YouthsEntered,
            ChildrenEntered = EditedInvitationEntry.ChildrenEntered,
            EntryDateTime = EditedInvitationEntry.EntryDateTime,
            QrCode = EditedInvitationEntry.QrCode
        };
        var responseHttp = await Repository.PutAsync<InvitationEntry>("api/InvitationEntry/full", invitationEntry);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        await SweetAlertService.FireAsync("Exito", "La entrada de invitacion se ha actualizada correctamente.", SweetAlertIcon.Success);
        IsModalVisible = false;
        CloseEditModal();
        await LoadInvitationEntries(currentPage);
    }

    private async Task SaveInvitationEntry()
    {
        InvitationEntry invitationEntry = new()
        {
            Code = Invitation!.Code,
            QrCode = qrResult,
            InvitationId = Invitation!.Id,
            AdultsEntered = NewInvitationEntry.AdultsEntered,
            YouthsEntered = NewInvitationEntry.YouthsEntered,
            ChildrenEntered = NewInvitationEntry.ChildrenEntered,
            EntryDateTime = NewInvitationEntry.EntryDateTime
        };
        var responseHttp = await Repository.PostAsync<InvitationEntry>("api/InvitationEntry/full", invitationEntry);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        await SweetAlertService.FireAsync("Exito", "La entrada de invitacion se ha guardado correctamente.", SweetAlertIcon.Success);
        IsModalVisible = false;
        await LoadInvitationEntries(currentPage);
    }
}