using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
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

        private CreateOrEditTablesDto createOrEditTablesDto = new();
        private AssignTablesDto AssignTablesDto = new();
        private GenerateTablesDto GenerateTablesDto = new();

        private bool modaAsignarMesa = false;
        private bool modaCrearOrEditaMesa = false;
        private bool modaGenerarMesa = false;
        private bool IsEditMode = false;

        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        private IEnumerable<Invitation> FilteredInvitation =>
            Invitations
                .Where(i => string.IsNullOrWhiteSpace(filterText) ||
                            i.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase) &&
                            i.Status == Status.Attend && i.TablesEvents == null
                )
                .Take(10);

        protected override async Task OnInitializedAsync()
        {
            await LoadTablesEventsAsync();
            await LoadInvitationsAsync();
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

        private async Task ConfirmDelete(TablesEvents table)
        {
            if (table.Invitation.Any())
            {
                var message = $"No se puede eliminar la mesa:{table.Name}. Porque aun tiene registros, primero elimine los invitados de la mesa.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "¿Eliminar esta mesa?",
                Text = $"Se eliminará '{table.Name}'. Esta acción no se puede deshacer.",
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
            }
        }

        private async Task ConfirmDeleteAssign(Invitation table)
        {
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "¿Eliminar invitados esta mesa?",
                Text = $"Se eliminará los invitados de esta mesa '{table.Name}'. Esta acción no se puede deshacer.",
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

                await LoadTablesEventsAsync();
                await LoadInvitationsAsync();
            }
        }

        private async Task ShowModaEditaMesa(TablesEvents updateTableEvent)
        {
            modaCrearOrEditaMesa = !modaCrearOrEditaMesa;
            createOrEditTablesDto.Seats = updateTableEvent.Seats;
            createOrEditTablesDto.Name = updateTableEvent.Name;
            createOrEditTablesDto.Description = updateTableEvent.Description;
            createOrEditTablesDto.EventoId = updateTableEvent.EventId;
            createOrEditTablesDto.Id = updateTableEvent.Id;

            IsEditMode = true;
        }

        private async Task ShowModaCrearMesa()
        {
            modaCrearOrEditaMesa = !modaCrearOrEditaMesa;
            IsEditMode = false;
        }

        private void CloseModaCrearOrEditaMesa()
        {
            modaCrearOrEditaMesa = !modaCrearOrEditaMesa;
        }

        private async Task ModaAsignarMes(TablesEvents tables)
        {
            modaAsignarMesa = !modaAsignarMesa;
            AssignTablesDto.TableId = tables.Id;
        }

        private void CloseModaAsignarMesa()
        {
            modaAsignarMesa = !modaAsignarMesa;
        }

        private async Task ModaGenerarMesa()
        {
            modaGenerarMesa = !modaGenerarMesa;
        }

        private void CloseModaGenerarMesa()
        {
            modaGenerarMesa = !modaGenerarMesa;
        }

        private async Task GenerarMesa()
        {
            HttpResponseWrapper<object>? responseHttp;
            GenerateTablesDto.EventoId = Invitations.FirstOrDefault()!.EventId;
            responseHttp = await Repository.PostAsync("api/Tables/Generate", GenerateTablesDto);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? $"No se pudo crear la mesa:{createOrEditTablesDto.Name}.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            CloseModaGenerarMesa();

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
                IsEditMode ? "Mesa actualizada con éxito." : "Mesa creada con éxito.",
                SweetAlertIcon.Success
            );
            GenerateTablesDto = new();
            await LoadTablesEventsAsync();
            await LoadInvitationsAsync();
        }

        private async Task SaveMesa()
        {
            HttpResponseWrapper<object>? responseHttp;
            createOrEditTablesDto.EventoId = Invitations.FirstOrDefault()!.EventId;

            if (IsEditMode)
            {
                // PUT -> Editar
                responseHttp = await Repository.PutAsync("api/Tables/full", createOrEditTablesDto);
            }
            else
            {
                // POST -> Crear
                responseHttp = await Repository.PostAsync("api/Tables/full", createOrEditTablesDto);
            }

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? $"No se pudo crear la mesa:{createOrEditTablesDto.Name}.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            CloseModaCrearOrEditaMesa();

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
                IsEditMode ? "Mesa actualizada con éxito." : "Mesa creada con éxito.",
                SweetAlertIcon.Success
            );
            createOrEditTablesDto = new();
            await LoadTablesEventsAsync();
            await LoadInvitationsAsync();
        }

        private async Task AssignTable()
        {
            //Validar que la mesa no tenga
            HttpResponseWrapper<object>? responseHttp;
            // POST -> Crear
            responseHttp = await Repository.PostAsync<AssignTablesDto>("api/Tables/Assign", AssignTablesDto);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo asignar la mesa.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            //Cerrar el modal
            CloseModaAsignarMesa();

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
                "Éxito", "Se ha asignado la mesa correctamente.",
                SweetAlertIcon.Success
            );
            AssignTablesDto = new();
            await LoadTablesEventsAsync();
            await LoadInvitationsAsync();
        }
    }
}