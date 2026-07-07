using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;
using System.Net;

namespace SIC.Frontend.Pages.MyEvents;
//ToDo: separar en componentes más pequeños, especialmente el modal de creación/edición de invitaciones
//ToDo: Revisar el reload después de cada acción, a veces es necesario y a veces no, optimizar eso

[Authorize(Roles = "Admin,WeddingPlanner,User")]
public partial class MyEventsDetails
{
    // Estados dinámicos por ID
    private int? loadingWhatsappId1;

    private string? SelectedTemplateName;
    private string SelectedTemplateDisplay = "Seleccionar plantilla";

    private Dictionary<int, bool> SelectedInvitations = new();
    private bool SelectAll = false;
    private bool IsSendingMassive = false;
    private bool IsCopyURl = false;
    private bool IsProcessList = false;

    private bool HasSelectedInvitations = false;

    private bool isLoadingWhatsapp = false;
    private int? loadingWhatsappId2;
    private int? copyingId1;
    private int? copyingId2;
    private int? copyingIdm1;
    private int? copyingIdm2;
    private string copyButtonText = "Copiar Invitación";
    private bool usarWhatsApp = true;
    private bool isSavingInvitation = false;
    private int currentPage = 1;
    private int totalPages;
    private bool isLoading = false;
    private bool isLoadingImport = false;
    private bool hasFileSelected = false;
    private IBrowserFile? selectedFile;
    private string? importResult;

    private Invitation NewInvitation = new();
    private bool IsModalVisible = false;
    private bool IsModalExcelVisible = false;
    private bool IsEditMode = false;
    private bool DeleteRegister = false;
    private DateTime MinAllowedDate { get; set; } = new DateTime(2023, 1, 1); // Sets January 1, 2023 as the minimum
    public Event? EventDetail { get; set; }
    public List<Invitation>? Invitations { get; set; }
    public List<WhatsAppTemplate>? Templates { get; set; }
    private List<TablesEvents> Tables = new();
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
    [Parameter, SupplyParameterFromQuery] public int RecordsNumber { get; set; } = 50;

    [Parameter] public string? Code { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadEvent();
        await LoadInvitations();
        await LoadTemplates();
        await LoadTablesEventsAsync();
    }

    private async Task LoadTablesEventsAsync()
    {
        var result = await Repository.GetAsync<List<TablesEvents>>($"api/Tables/{EventDetail.Id}");
        if (result.Error)
        {
            var message = await result.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        Tables = result.Response ?? new List<TablesEvents>();
    }

    private async Task LoadTemplates()
    {
        var url = $"api/whatsapp/get-templates";

        if (!string.IsNullOrWhiteSpace(Filter))
        {
            url += $"&Filter={Filter}";
        }

        //.GetAsync<List<Invitation>>
        var responseHttp = await Repository.GetAsync<List<WhatsAppTemplate>>(url);
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        // Backend ya devuelve total de páginas, no de registros
        Templates = responseHttp.Response ?? new List<WhatsAppTemplate>();
    }

    private async Task SelectedPageAsync(int page)
    {
        currentPage = page;
        await LoadInvitations(currentPage);
    }

    private void SeleccionarTemplate(WhatsAppTemplate template)
    {
        SelectedTemplateName = template.Name;
        SelectedTemplateDisplay = template.DisplayName;
    }

    private async Task ShowCreateModal()
    {
        NewInvitation = new Invitation();
        NewInvitation.EventId = EventDetail!.Id;
        IsEditMode = false;
        IsModalVisible = true;
    }

    private void ShowModalExcel()
    {
        IsModalExcelVisible = true;
    }

    private void CloseModalExcel()
    {
        IsModalExcelVisible = false;
    }

    private void NavegateToControlTables()
    {
        NavigationManager.NavigateTo($"/events/tables/{EventDetail!.Code}");
    }

    private void NavegateToMessage()
    {
        NavigationManager.NavigateTo($"/events/message-events/{EventDetail!.Code}");
    }

    private void AddGuest()
    {
        //Cuando le de a agregar actualiza el contador de NumberAdults y hacer un changed en el select cuando se cambie el tipo de invitado
        NewInvitation.Guests ??= new List<InvitationGuest>();

        NewInvitation.Guests.Add(new InvitationGuest
        {
            GuestName = $"Invitado {NewInvitation.Guests.Count + 1}",           // permitido
            GuestType = GuestType.Adult,
            InvitationId = NewInvitation.Id,
            Invitation = null,

            // SIEMPRE null para evitar validaciones
        });
        UpdateGuestCounters(NewInvitation.Guests);
    }

    private void OnStatusChanged()
    {
        foreach (var guest in NewInvitation.Guests)
        {
            guest.Status = NewInvitation.Status;
        }
        UpdateGuestStatusCounters(NewInvitation.Guests);
    }

    // Método para manejar el cambio de tipo de invitado
    private void OnGuestTypeChanged()
    {
        UpdateGuestCounters(NewInvitation.Guests);
    }

    private void OnGuestStatusChanged()
    {
        UpdateGuestStatusCounters(NewInvitation.Guests);
    }

    private void UpdateGuestStatusCounters(ICollection<InvitationGuest> guests)
    {
        NewInvitation.NumberConfirmedAdults = guests.Count(g => g.GuestType == GuestType.Adult && g.Status == Status.Attend);
        NewInvitation.NumberConfirmedYouths = guests.Count(g => g.GuestType == GuestType.Youth && g.Status == Status.Attend);
        NewInvitation.NumberConfirmedChildren = guests.Count(g => g.GuestType == GuestType.Children && g.Status == Status.Attend);
    }

    private void UpdateGuestCounters(ICollection<InvitationGuest> guests)
    {
        NewInvitation.NumberAdults = guests.Count(g => g.GuestType == GuestType.Adult);
        NewInvitation.NumberYouths = guests.Count(g => g.GuestType == GuestType.Youth);
        NewInvitation.NumberChildren = guests.Count(g => g.GuestType == GuestType.Children);
    }

    private void RemoveGuest(InvitationGuest guest)
    {
        NewInvitation.Guests.Remove(guest);
        UpdateGuestCounters(NewInvitation.Guests);
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
            NumberYouths = invitation.NumberYouths,
            NumberChildren = invitation.NumberChildren,
            NumberConfirmedAdults = invitation.NumberConfirmedAdults,
            NumberConfirmedYouths = invitation.NumberConfirmedYouths,
            NumberConfirmedChildren = invitation.NumberConfirmedChildren,
            Guests = invitation.Guests,
            Table = invitation.Table,
            Comments = invitation.Comments,
            CountryCode = invitation.CountryCode,
            SentDate = invitation.SentDate,
            ConfirmationDate = invitation.ConfirmationDate,
            Name = invitation.Name,
            TablesEvents = invitation.TablesEvents,
            TablesEventsId = invitation.TablesEventsId,
            Status = invitation.Status
        };
        IsEditMode = true;
        IsModalVisible = true;
    }

    private void CloseModal()
    {
        IsModalVisible = false;
    }

    private async Task DeleteInvitation()
    {
        // 🔹 Confirmación antes de eliminar
        var confirmResult = await SweetAlertService.FireAsync(new SweetAlertOptions
        {
            Title = "¿Eliminar invitación?",
            Text = "Esta acción no se puede deshacer. ¿Deseas continuar?",
            Icon = SweetAlertIcon.Warning,
            ShowCancelButton = true,
            ConfirmButtonText = "Sí, eliminar",
            CancelButtonText = "Cancelar",
            ConfirmButtonColor = "#d33",
            CancelButtonColor = "#3085d6"
        });

        // Si el usuario cancela, no hacer nada
        if (confirmResult.IsDismissed)
            return;

        // 🔹 Proceder con la eliminación
        var responseHttp = await Repository.DeleteAsync<object>($"api/Invitations/{NewInvitation.Id}");
        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar la invitación.";
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }

        CloseModal();

        // 🔹 Mostrar notificación tipo toast
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
            "Invitación eliminada con éxito.",
            SweetAlertIcon.Success
        );

        await LoadEvent();
        await LoadInvitations(currentPage);
    }

    private async Task SaveInvitation()
    {
        HttpResponseWrapper<object>? responseHttp;
        isSavingInvitation = true;

        if (NewInvitation.TablesEventsId.HasValue)
        {
            var table = Tables.FirstOrDefault(t => t.Id == NewInvitation.TablesEventsId);

            if (table != null &&
                (table.Seats - table.OccupiedSeats) <
                NewInvitation.Guests.Count(x => x.Status == Status.Attend))
            {
                await SweetAlertService.FireAsync(
                    "Error",
                    "No se pudo guardar la invitación. La cantidad de invitados es mayor a la cantidad de lugares disponibles en la mesa, favor de cambiar de mesa o adicionar más lugares.",
                    SweetAlertIcon.Error);
                isSavingInvitation = false;

                return;
            }
        }
        else
        {
            NewInvitation.TablesEvents = null;
        }

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
            var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar la Inivitacion.";
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            isSavingInvitation = false;
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
            IsEditMode ? "Inivitacion actualizada con éxito." : "Inivitacion creada con éxito.",
            SweetAlertIcon.Success
        );
        isSavingInvitation = false;
        await LoadEvent();
        await LoadInvitations();
        await LoadTablesEventsAsync();
    }

    private async Task DescargarExcel()
    {
        try
        {
            isLoading = true;
            var content = await Repository.GetFileAsync($"api/excel/GenerarExcel/{EventDetail!.Id}");

            if (content.Length > 0)
            {
                await JsRuntime.DownloadFileAsync($"{EventDetail.Name}.xlsx", content);
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task GeneratedPDF()
    {
        try
        {
            IsProcessList = true;

            var content = await Repository.GetFileAsync($"api/Invitations/generatedpdf?evento={EventDetail!.Code}");

            if (content != null && content.Length > 0)
            {
                await JsRuntime.DownloadFileAsync(
                    $"{EventDetail.Name}.pdf",
                    content,
                    "application/pdf");
            }
            else
            {
                await SweetAlertService.FireAsync("Generar .PDF", "No tienes ningun invitado confirmado aun, espere a que los invitados confirmen para generar la lista de invitados", SweetAlertIcon.Info);
            }
        }
        finally
        {
            IsProcessList = false;
        }
    }

    private async Task CopiarEventUrl()
    {
        IsCopyURl = true;
        var url = $"{NavigationManager.BaseUri}status-events/{Code}";

        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", url);
        await Task.Delay(1500);
        IsCopyURl = false;
    }

    private async Task AbrirWhatsapp(string phoneNumber, string code, int invitationId, int column)
    {
        string mensaje;
        if (column == 1)
        {
            loadingWhatsappId1 = invitationId;
        }
        else
        {
            loadingWhatsappId2 = invitationId;
        }

        var responseHttp = await Repository.GetAsync<SIC.Shared.Entities.Message>($"api/Messages/byCode/{Code}/{code}");

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        if (responseHttp.Response != null)
        {
            if (column == 1)
            {
                mensaje = responseHttp.Response.MessageInvitation;
            }
            else
            {
                mensaje = responseHttp.Response.MessageConfirmation;
            }
        }
        else
        {
            await SweetAlertService.FireAsync("Error", "No se encontró el mensaje de invitación.", SweetAlertIcon.Error);
            if (column == 1)
                copyingId1 = null;
            else
                copyingId2 = null;
            copyButtonText = "Copiar Invitación";
            return;
        }
        var url = $"https://wa.me/{phoneNumber}?text={Uri.EscapeDataString(mensaje)}";

        await JsRuntime.InvokeVoidAsync("window.open", url, "_blank");

        await Task.Delay(1000); // pequeña pausa solo visual
        if (column == 1)
            loadingWhatsappId1 = null;
        else
            loadingWhatsappId2 = null;
    }

    private async Task CopiarInvitacion(string codeinvitation, int invitationId, int column)
    {
        string mensaje;

        if (column == 1)
        {
            copyingId1 = invitationId; // ✅ AQUÍ estaba el error
        }
        else
        {
            copyingId2 = invitationId;
        }

        StateHasChanged(); // 🔥 fuerza render inmediato

        var responseHttp = await Repository.GetAsync<SIC.Shared.Entities.Message>(
            $"api/Messages/byCode/{Code}/{codeinvitation}");

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            ResetCopyState(column);
            return;
        }

        if (responseHttp.Response == null)
        {
            await SweetAlertService.FireAsync(
                "Error",
                "No se encontró el mensaje de invitación.",
                SweetAlertIcon.Error);

            ResetCopyState(column);
            return;
        }

        mensaje = column == 1
            ? responseHttp.Response.MessageInvitation
            : responseHttp.Response.MessageConfirmation;

        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", mensaje);

        await Task.Delay(1500);

        ResetCopyState(column);
    }

    private async Task CopiarInvitacionm(string codeinvitation, int invitationId, int column)
    {
        string mensaje;

        if (column == 1)
        {
            copyingIdm1 = invitationId; // ✅ AQUÍ estaba el error
        }
        else
        {
            copyingIdm2 = invitationId;
        }

        StateHasChanged(); // 🔥 fuerza render inmediato

        var responseHttp = await Repository.GetAsync<SIC.Shared.Entities.Message>(
            $"api/Messages/byCode/{Code}/{codeinvitation}");

        if (responseHttp.Error)
        {
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            ResetCopyState(column);
            return;
        }

        if (responseHttp.Response == null)
        {
            await SweetAlertService.FireAsync(
                "Error",
                "No se encontró el mensaje de invitación.",
                SweetAlertIcon.Error);

            ResetCopyState(column);
            return;
        }

        mensaje = column == 1
            ? responseHttp.Response.MessageInvitation
            : responseHttp.Response.MessageConfirmation;

        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", mensaje);

        await Task.Delay(1500);

        if (column == 1)
        {
            copyingIdm1 = null; // ✅ AQUÍ estaba el error
        }
        else
        {
            copyingIdm2 = null;
        }

        ResetCopyState(column);
    }

    private void ResetCopyState(int column)
    {
        if (column == 1)
            copyingId1 = null;
        else
            copyingId2 = null;

        StateHasChanged();
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
                        $"api/excel/ImportarExcel/{EventDetail!.Id}/{DeleteRegister}",
                        stream,
                        selectedFile.Name
                    );
                if (!responseHttp.Error)
                {
                    var result = responseHttp.Response;
                    importResult = $"✅ Archivo procesado: {result}";
                    /*await SweetAlertService.FireAsync("Invitaciones",

                            $"Agregadas: {result!.Agregadas}\n" +
                            $"Modificadas: {result!.Modificadas}\n" +
                            $"Errores: {result!.Errores}\n" +
                            $"Total procesadas: {result!.Total}\n\n" +
                            "✅ Las invitaciones se actualizaron correctamente",
                            SweetAlertIcon.Info);*/
                    await SweetAlertService.FireAsync(new SweetAlertOptions
                    {
                        Title = "Invitaciones",
                        Html = $@"<div style='text-align:left; font-size:0.95rem; line-height:1.4;'>
                                <ul style='padding-left:1.2rem; margin:0 0 0.6rem 0;'>
                                    <li><strong>Agregadas:</strong> {result!.Agregadas}</li>
                                    <li><strong>Modificadas:</strong> {result!.Modificadas}</li>
                                    <li><strong>Eliminadas:</strong> {result!.Eliminadas} </li>
                                    <li><strong>Errores:</strong> {result!.Errores}</li>
                                    <li><strong> Total procesadas:</strong> {result!.Total}</li>
                                </ul>
                                <p style = 'margin-top:0.6rem;'>✅ <strong> Las invitaciones se actualizaron correctamente </strong></p>
                            </div> ",
                        Icon = SweetAlertIcon.Success,
                        ConfirmButtonText = "Aceptar"
                    });
                    CloseModalExcel();
                    await LoadInvitations();
                }
                else
                {
                    var error = responseHttp.Error;
                    importResult = $"❌ Error: {error}";
                    await SweetAlertService.FireAsync("Error", "Ha ocurrido un error mientras se subia la lista de invitados, favor de revisar el excel, y no le cambie o elimine ninguna columna.", SweetAlertIcon.Error);
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

        SelectedInvitations = Invitations!
        .ToDictionary(i => i.Id, _ => false);
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

    private async Task EnviarInvitacion(string code, string template, int invitationId, string displayedName)
    {
        try
        {
            loadingWhatsappId1 = invitationId;
            var confirmResult = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Html = $@"
                        <div style='text-align:center'>
                            <p>Estas seguro de enviar la plantilla:</p>
                            <h2 style='color:#3C6A79'><strong>{displayedName}</strong></h2>
                            <p>¿Deseas continuar?</p>
                        </div>
                    ",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, Continuar",
                CancelButtonText = "Cancelar",
                ConfirmButtonColor = "#3C6A79",
                CancelButtonColor = "#d33"
            });

            if (confirmResult.IsDismissed)
                return;
            var codes = new List<string> { code };

            var dto = new MasiveSendTemplateDTO
            {
                TemplateName = template,
                Codes = codes
            };

            var response = await Repository.PostAsync<object>(
                "api/whatsapp/enviar-invitacion-dina",
                dto
            );

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                var errorMessage = JsonConvert.DeserializeObject<WhatsAppApiError>(message!);

                await SweetAlertService.FireAsync(
                    "Error",
                    errorMessage!.error.message,
                    SweetAlertIcon.Error
                );
                return;
            }

            await SweetAlertService.FireAsync(
                "Éxito",
                "Invitación enviada correctamente",
                SweetAlertIcon.Success
            );
        }
        catch
        {
            await SweetAlertService.FireAsync(
                "Error",
                "Algo ocurrió, inténtalo más tarde",
                SweetAlertIcon.Error
            );
        }
        finally
        {
            loadingWhatsappId1 = null;
            await LoadInvitations(currentPage);
            StateHasChanged();
        }
    }

    private async Task EnviarInvitacionesMasivas(WhatsAppTemplate templateName)
    {
        try
        {
            IsSendingMassive = true;
            var confirmResult = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Html = $@"
                        <div style='text-align:center'>
                            <p>Estas seguro de enviar la plantilla:</p>
                            <h2 style='color:#3C6A79'><strong>{templateName.DisplayName}</strong></h2>
                            <p>¿Deseas continuar?</p>
                        </div>
                    ",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, Continuar",
                CancelButtonText = "Cancelar",
                ConfirmButtonColor = "#3C6A79",
                CancelButtonColor = "#d33"
            });

            if (confirmResult.IsDismissed)
                return;

            var seleccionados = Invitations!
                .Where(i => SelectedInvitations.TryGetValue(i.Id, out var selected) && selected)
                .Select(i => i.Code!)
                .ToList();

            if (!seleccionados.Any())
                return;

            var dto = new MasiveSendTemplateDTO
            {
                TemplateName = templateName.Name, // 👈 ahora dinámico
                Codes = seleccionados
            };

            var response = await Repository.PostAsync<object>(
                "api/whatsapp/enviar-invitacion-dina",
                dto
            );

            if (response.Error)
            {
                var msg = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync(
                    "Error",
                    msg,
                    SweetAlertIcon.Error
                );
                return;
            }

            await SweetAlertService.FireAsync(
                "Envío masivo finalizado",
                $"Invitaciones enviadas: {seleccionados.Count}",
                SweetAlertIcon.Success
            );
        }
        catch
        {
            await SweetAlertService.FireAsync(
                "Error",
                "Algo ocurrió, inténtalo más tarde",
                SweetAlertIcon.Error
            );
        }
        finally
        {
            HasSelectedInvitations = false;
            SelectAll = false;
            foreach (var key in SelectedInvitations.Keys.ToList())
                SelectedInvitations[key] = false;

            IsSendingMassive = false;
            await LoadInvitations(currentPage);
            StateHasChanged();
        }
    }

    //ToDo: que sea acumulativos
    private void ToggleSelectAll(bool value)
    {
        SelectAll = value;

        foreach (var key in SelectedInvitations.Keys.ToList())
            SelectedInvitations[key] = value;

        HasSelectedInvitations = value;

        StateHasChanged();
    }

    private void OnItemSelectionChanged(int id, bool value)
    {
        SelectedInvitations[id] = value;

        HasSelectedInvitations = SelectedInvitations.Values.Any(v => v);
        SelectAll = SelectedInvitations.Values.All(v => v);

        StateHasChanged();
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

    private string GetStatusBadge(Status status) => status switch
    {
        Status.Attend => "success",
        Status.NotAttend => "danger",
        _ => "secondary"
    };
}