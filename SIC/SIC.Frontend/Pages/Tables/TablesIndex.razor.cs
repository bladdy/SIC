using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Helpers;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

namespace SIC.Frontend.Pages.Tables
{
    public partial class TablesIndex
    {
        [Parameter] public string? Code { get; set; }
        private int currentPage = 1;
        private int totalPages = 2;
        private TablesEvents? Table { get; set; }
        private List<Invitation> Invitations = new();

        private List<TablesEvents>? Tables { get; set; }
        private string filterText = string.Empty;
        private string filterGuestText = string.Empty;
        private string asignacionModo = "herencia";
        private HashSet<int> selectedGuestIds = new();
        private HashSet<int> selectedInvitationIds = new();
        private List<InvitationGuest> allEventGuests = new();

        private CreateOrEditTablesDto createOrEditTablesDto = new();
        private AssignTablesDto AssignTablesDto = new();
        private GenerateTablesDto GenerateTablesDto = new();

        private bool modaAsignarMesa = false;
        private bool modaCrearOrEditaMesa = false;
        private bool modaGenerarMesa = false;
        private bool IsEditMode = false;

        private bool isReloading = false;
        private bool isSavingAssignment = false;
        private string busyMessage = string.Empty;
        private bool IsBusy => isReloading || isSavingAssignment;

        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private bool isGeneratingPdf = false;

        private IEnumerable<Invitation> FilteredInvitation =>
            Invitations
                .Where(i => i.Status == Status.Attend && i.TablesEventsId == null)
                .Where(i => i.Guests?.Any(g => g.Status == Status.Attend && !g.TablesEventsId.HasValue) == true)
                .Where(i => string.IsNullOrWhiteSpace(filterText) ||
                            i.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(i => i.Name);

        private IEnumerable<InvitationGuest> FilteredGuestsForAssignment =>
            allEventGuests
                .Where(g => g.Status == Status.Attend)
                .Where(g => !g.TablesEventsId.HasValue &&
                            !(invitationsById.TryGetValue(g.InvitationId, out var inv) && inv.TablesEventsId != null))
                .Where(g => string.IsNullOrWhiteSpace(filterGuestText) ||
                            g.GuestName.Contains(filterGuestText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(g => g.GuestName);

        private int totalTables;
        private int totalSeats;
        private int occupiedSeats;
        private int availableSeats;
        private int eventId;
        private string eventName = string.Empty;
        private Dictionary<int, Invitation> invitationsById = new();

        protected override async Task OnInitializedAsync()
        {
            var tablesTask = LoadTablesEventsAsync();
            var invitationsTask = LoadInvitationsAsync();
            var eventInfoTask = LoadEventInfoAsync();
            await Task.WhenAll(tablesTask, invitationsTask, eventInfoTask);
            ComputeStats();
        }

        private void ComputeStats()
        {
            totalTables = Tables?.Count ?? 0;
            totalSeats = Tables?.Sum(s => s.Seats) ?? 0;
            occupiedSeats = Tables?.Sum(s => s.OccupiedSeats) ?? 0;
            availableSeats = totalSeats - occupiedSeats;
        }

        private async Task ReloadDataAsync()
        {
            isReloading = true;
            busyMessage = "Actualizando mesas...";
            StateHasChanged();
            try
            {
                var tablesTask = LoadTablesEventsAsync();
                var invitationsTask = LoadInvitationsAsync();
                await Task.WhenAll(tablesTask, invitationsTask);
                ComputeStats();
            }
            finally
            {
                isReloading = false;
            }
        }

        private async Task LoadEventInfoAsync()
        {
            var result = await Repository.GetAsync<EventInfoDto>($"api/Events/infobycode/{Code}");
            if (!result.Error && result.Response != null)
            {
                eventId = result.Response.Id;
                eventName = result.Response.Name;
            }
        }

        private async Task LoadInvitationsAsync()
        {
            var result = await Repository.GetAsync<List<Invitation>>($"api/Invitations/byEventCode/{Code}");
            if (result.Error)
            {
                var message = await result.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            Invitations = result.Response ?? new List<Invitation>();
            invitationsById = Invitations.ToDictionary(i => i.Id);
        }

        private async Task LoadTablesEventsAsync()
        {
            var result = await Repository.GetAsync<List<TablesEvents>>($"api/Tables/tablesbycode/{Code}");
            if (result.Error)
            {
                var message = await result.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            Tables = result.Response ?? new List<TablesEvents>();
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
        }

        private async Task GeneratePdfAsync()
        {
            isGeneratingPdf = true;
            try
            {
                var nombre = string.IsNullOrWhiteSpace(eventName) ? Code : eventName;
                var content = await Repository.GetFileAsync($"api/Tables/generatedpdf?code={Code}&evento={Uri.EscapeDataString(nombre ?? "")}");
                if (content != null && content.Length > 0)
                {
                    await JS.DownloadFileAsync($"mesas-{nombre}.pdf", content, "application/pdf");
                }
                else
                {
                    await SweetAlertService.FireAsync("Error", "No hay mesas para generar el PDF.", SweetAlertIcon.Error);
                }
            }
            finally
            {
                isGeneratingPdf = false;
            }
        }

        private async Task ConfirmDelete(TablesEvents table)
        {
            if (table.Invitations.Any())
            {
                var message = $"No se puede eliminar la mesa:{table.Name}. Porque aun tiene registros, primero elimine los invitados de la mesa.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Eliminar esta mesa?",
                Text = $"Se eliminara '{table.Name}'. Esta accion no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true
            });

            if (!string.IsNullOrEmpty(result.Value))
                await DeleTable(table);
        }

        private async Task DeleTable(TablesEvents table)
        {
            var response = await Repository.DeleteAsync<TablesEvents>($"api/Tables/{table.Id}");
            if (!response.Error)
            {
                await SweetAlertService.FireAsync("Eliminado", "Mesa eliminada correctamente.", SweetAlertIcon.Success);

                await LoadTablesEventsAsync();
                ComputeStats();
            }
        }

        private async Task ConfirmDeleteAssign(Invitation table)
        {
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "�Eliminar invitados esta mesa?",
                Text = $"Se eliminar� los invitados de esta mesa '{table.Name}'. Esta acci�n no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true
            });

            if (!string.IsNullOrEmpty(result.Value))
                await DeleteAssing(table);
        }

        private async Task DeleteAssing(Invitation table)
        {
            var response = await Repository.DeleteAsync<TablesEvents>($"api/Tables/delete/{table.Id}");
            if (!response.Error)
            {
                await SweetAlertService.FireAsync("Eliminado", "Eliminiado invitados de esta mesa correctamente.", SweetAlertIcon.Success);

                await ReloadDataAsync();
            }
        }

        private async Task UnassignGuest(InvitationGuest guest)
        {
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Desasignar mesa",
                Text = $"Quitar mesa individual a '{guest.GuestName}'? Usara la mesa de su invitacion.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                var response = await Repository.DeleteAsync<bool>($"api/Tables/UnassignGuest/{guest.Id}");
                if (response.Error)
                {
                    var message = await response.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    return;
                }
                await SweetAlertService.FireAsync("Exito", "Mesa individual removida.", SweetAlertIcon.Success);
                await ReloadDataAsync();
            }
        }

        private void ShowModaEditaMesa(TablesEvents updateTableEvent)
        {
            createOrEditTablesDto = new CreateOrEditTablesDto
            {
                Id = updateTableEvent.Id,
                EventoId = updateTableEvent.EventId,
                Name = updateTableEvent.Name,
                Description = updateTableEvent.Description,
                Seats = updateTableEvent.Seats
            };

            IsEditMode = true;
            modaCrearOrEditaMesa = true;
        }

        private void ShowModaCrearMesa()
        {
            createOrEditTablesDto = new CreateOrEditTablesDto();
            IsEditMode = false;
            modaCrearOrEditaMesa = true;
        }

        private void CloseModaCrearOrEditaMesa()
        {
            modaCrearOrEditaMesa = false;
        }

        private async Task ModaAsignarMes(TablesEvents tables)
        {
            Table = tables;
            modaAsignarMesa = true;
            asignacionModo = "herencia";
            filterText = string.Empty;
            filterGuestText = string.Empty;
            selectedGuestIds.Clear();
            selectedInvitationIds.Clear();
            AssignTablesDto.TableId = tables.Id;
            AssignTablesDto.InvitationId = 0;
            allEventGuests = Invitations
                .SelectMany(i => i.Guests ?? Enumerable.Empty<InvitationGuest>())
                .ToList();
        }

        private void CloseModaAsignarMesa()
        {
            modaAsignarMesa = false;
            asignacionModo = "herencia";
            filterText = string.Empty;
            filterGuestText = string.Empty;
            selectedGuestIds.Clear();
            selectedInvitationIds.Clear();
            Table = null;
        }

        private void ToggleGuestSelection(int guestId)
        {
            if (!selectedGuestIds.Remove(guestId))
            {
                selectedGuestIds.Add(guestId);
            }
        }

        private void ToggleInvitationSelection(int invitationId)
        {
            if (!selectedInvitationIds.Remove(invitationId))
            {
                selectedInvitationIds.Add(invitationId);
            }
        }

        private void ModaGenerarMesa()
        {
            GenerateTablesDto = new GenerateTablesDto();
            modaGenerarMesa = true;
        }

        private void CloseModaGenerarMesa()
        {
            modaGenerarMesa = false;
        }

        private async Task GenerarMesa()
        {
            if (eventId == 0)
            {
                await SweetAlertService.FireAsync("Error", "No se pudo determinar el evento.", SweetAlertIcon.Error);
                return;
            }

            GenerateTablesDto.EventoId = eventId;
            var responseHttp = await Repository.PostAsync("api/Tables/Generate", GenerateTablesDto);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudieron generar las mesas.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            modaGenerarMesa = false;
            GenerateTablesDto = new();

            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync("Éxito", "Mesas generadas con éxito.", SweetAlertIcon.Success);

            await ReloadDataAsync();
        }

        private async Task SaveMesa()
        {
            if (eventId == 0)
            {
                await SweetAlertService.FireAsync("Error", "No se pudo determinar el evento.", SweetAlertIcon.Error);
                return;
            }

            createOrEditTablesDto.EventoId = eventId;

            var responseHttp = IsEditMode
                ? await Repository.PutAsync("api/Tables/full", createOrEditTablesDto)
                : await Repository.PostAsync("api/Tables/full", createOrEditTablesDto);

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? $"No se pudo guardar la mesa:{createOrEditTablesDto.Name}.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            modaCrearOrEditaMesa = false;
            createOrEditTablesDto = new();

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
                IsEditMode ? "Mesa actualizada con éxito." : "Mesa creada con éxito.",
                SweetAlertIcon.Success
            );

            await ReloadDataAsync();
        }

        private string GetTableStatusClass(TablesEvents table)
        {
            if (table.Seats == 0) return "bg-secondary";
            double percentage = (double)table.OccupiedSeats / table.Seats;
            if (percentage > 1.0) return "bg-danger";
            if (percentage >= 0.9) return "bg-warning text-dark";
            if (percentage >= 0.5) return "bg-info";
            if (percentage > 0) return "bg-primary";
            return "bg-success";
        }

        private string GetTableStatusLabel(TablesEvents table)
        {
            if (table.Seats == 0) return "Sin lugares";
            double percentage = (double)table.OccupiedSeats / table.Seats;
            if (percentage > 1.0) return "Sobre capacidad";
            if (percentage >= 0.9) return "Casi llena";
            if (percentage >= 0.5) return "Disponible";
            if (percentage > 0) return "En uso";
            return "Libre";
        }

        private int GetEffectiveGuestCount(TablesEvents table)
        {
            int count = 0;
            foreach (var inv in table.Invitations)
            {
                count += inv.Guests?.Count(g => g.Status == SIC.Shared.Enums.Status.Attend) ?? 0;
            }
            count += table.Guests?.Count(g => g.Status == SIC.Shared.Enums.Status.Attend) ?? 0;
            return count;
        }

        private async Task AssignTable()
        {
            if (asignacionModo == "individual")
            {
                var available = Table!.Seats - Table.OccupiedSeats;
                if (selectedGuestIds.Count > available)
                {
                    await SweetAlertService.FireAsync("Error",
                        $"No hay suficientes lugares. Disponibles: {available}, seleccionados: {selectedGuestIds.Count}.",
                        SweetAlertIcon.Error);
                    return;
                }

                var dtos = selectedGuestIds
                    .Select(guestId => new AssignGuestTableDto { GuestId = guestId, TablesEventsId = Table.Id })
                    .ToList();

                modaAsignarMesa = false;
                isSavingAssignment = true;
                busyMessage = "Guardando asignación...";
                StateHasChanged();

                try
                {
                    var response = await Repository.PostAsync<List<AssignGuestTableDto>, AssignBulkResultDto>(
                        "api/Tables/AssignGuestBulk", dtos);

                    if (response.Error)
                    {
                        var message = await response.GetErrorMessageAsync();
                        await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    }
                    else
                    {
                        var assigned = response.Response?.Assigned ?? 0;
                        var skipped = response.Response?.Skipped ?? new List<string>();
                        if (skipped.Count > 0)
                        {
                            await SweetAlertService.FireAsync("Advertencia",
                                $"Asignados: {assigned}. Omitidos: {string.Join(", ", skipped)}.",
                                SweetAlertIcon.Warning);
                        }
                        else
                        {
                            var toast = SweetAlertService.Mixin(new SweetAlertOptions
                            {
                                Toast = true,
                                Position = SweetAlertPosition.TopEnd,
                                ShowConfirmButton = false,
                                Timer = 3000,
                                TimerProgressBar = true,
                            });
                            await toast.FireAsync("Exito",
                                $"Se asignaron {assigned} invitado(s) correctamente.",
                                SweetAlertIcon.Success);
                        }
                    }
                }
                finally
                {
                    isSavingAssignment = false;
                }
            }
            else
            {
                var totalGuests = selectedInvitationIds.Sum(invId =>
                    invitationsById.TryGetValue(invId, out var inv)
                        ? inv.Guests?.Count(g => g.Status == Status.Attend) ?? 0
                        : 0);

                var available = Table!.Seats - Table.OccupiedSeats;
                if (totalGuests > available)
                {
                    await SweetAlertService.FireAsync("Error",
                        $"No hay suficientes lugares. Disponibles: {available}, total invitados de las seleccionadas: {totalGuests}.",
                        SweetAlertIcon.Error);
                    return;
                }

                var dtos = selectedInvitationIds
                    .Select(invId => new AssignTablesDto { InvitationId = invId, TableId = Table.Id })
                    .ToList();

                modaAsignarMesa = false;
                isSavingAssignment = true;
                busyMessage = "Guardando asignación...";
                StateHasChanged();

                try
                {
                    var response = await Repository.PostAsync<List<AssignTablesDto>, AssignBulkResultDto>(
                        "api/Tables/AssignBulk", dtos);

                    if (response.Error)
                    {
                        var message = await response.GetErrorMessageAsync();
                        await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                    }
                    else
                    {
                        var assigned = response.Response?.Assigned ?? 0;
                        var skipped = response.Response?.Skipped ?? new List<string>();
                        if (skipped.Count > 0)
                        {
                            await SweetAlertService.FireAsync("Advertencia",
                                $"Asignadas: {assigned}. Omitidas: {string.Join(", ", skipped)}.",
                                SweetAlertIcon.Warning);
                        }
                        else
                        {
                            var toast = SweetAlertService.Mixin(new SweetAlertOptions
                            {
                                Toast = true,
                                Position = SweetAlertPosition.TopEnd,
                                ShowConfirmButton = false,
                                Timer = 3000,
                                TimerProgressBar = true,
                            });
                            await toast.FireAsync("Exito",
                                $"Se asignaron {assigned} invitacion(es) correctamente.",
                                SweetAlertIcon.Success);
                        }
                    }
                }
                finally
                {
                    isSavingAssignment = false;
                }
            }

            AssignTablesDto = new();
            await ReloadDataAsync();
        }
    }
}