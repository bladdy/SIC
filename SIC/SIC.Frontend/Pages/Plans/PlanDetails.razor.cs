using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.Plans
{
    public partial class PlanDetails
    {
        [Parameter] public string? Id { get; set; }
        [Inject] private IRepository repository { get; set; } = default!;
        private bool _isLoading = true;

        private List<int> selectedItems = new();
        public List<Item>? Items { get; set; }
        public Plan? Plan { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataAsync();
            await LoadPlanDataAsync();
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

        private void AgregarAlPlan()
        {
            if (selectedItems.Count == 0)
            {
                Console.WriteLine("Debes seleccionar al menos un ítem.");
                return;
            }

            Console.WriteLine($"Ítems seleccionados: {string.Join(", ", selectedItems)}");
            // Aquí puedes guardar los items seleccionados al plan en tu API o DB
        }
    }
}