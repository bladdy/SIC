using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.Plans
{
    public partial class PlanDetails
    {
        [Parameter] public string? Id { get; set; }
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private bool _isLoading = true;

        private List<int> selectedItems = new();
        public List<Item>? Items { get; set; }
        public List<PlanItem>? PlanItems { get; set; } = new();
        public Plan? Plan { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            await LoadPlanDataAsync();
            await LoadPlanItemsDataAsync();

            //Inicializar ítems seleccionados según los que vienen del plan
            if (PlanItems != null && PlanItems.Any())
            {
                selectedItems = PlanItems
                    .Where(pi => pi.ItemId > 0)
                    .Select(pi => pi.ItemId)
                    .ToList();
            }

            _isLoading = false;
        }

        private async Task LoadDataAsync()
        {
            var responseHttp = await repository.GetAsync<List<Item>>("api/Items");
            if (!responseHttp.Error && responseHttp.Response != null)
            {
                Items = responseHttp.Response;
            }
        }

        private async Task LoadPlanDataAsync()
        {
            var responseHttp = await repository.GetAsync<Plan>($"api/Plans/{Convert.ToInt32(Id)}");
            if (!responseHttp.Error && responseHttp.Response != null)
            {
                Plan = responseHttp.Response;
            }
        }

        private async Task LoadPlanItemsDataAsync()
        {
            var responseHttp = await repository.GetAsync<List<PlanItem>>($"api/PlanItem/Item-By-Plan/{Convert.ToInt32(Id)}");
            if (!responseHttp.Error && responseHttp.Response != null)
            {
                PlanItems = responseHttp.Response;
            }
        }

        private void ToggleSelection(int id, object? isChecked)
        {
            bool selected = isChecked is bool value && value;
            if (selected)
            {
                if (!selectedItems.Contains(id))
                    selectedItems.Add(id);
            }
            else
            {
                selectedItems.Remove(id);
            }
        }

        private async Task AgregarAlPlan()
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                await SweetAlertService.FireAsync("Aviso", "Debes seleccionar al menos un ítem.", SweetAlertIcon.Warning);
                return;
            }

            // Enviamos directamente la lista de IDs como JSON
            var responseHttp = await repository.PostAsync<List<int>>($"api/PlanItem/full/{Id}", selectedItems);

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo asignar los ítems al plan.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            await SweetAlertService.FireAsync("Éxito", "Los ítems se han asignado correctamente.", SweetAlertIcon.Success);
            NavigationManager.NavigateTo("/plans");
        }
    }
}