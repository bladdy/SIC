using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;

namespace SIC.Frontend.Pages.Products
{
    public partial class ProductsIndex
    {
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

        public List<Product>? Products { get; set; }
        private Product NewProduct = new();
        private bool IsModalVisible = false;
        private bool IsEditMode = false;  // Nuevo flag

        private Task OnAmountChanged(int value)
        {
            NewProduct.Amount = value;
            CalcularPrecio();
            return Task.CompletedTask;
        }

        private Task OnTotalChanged(decimal? value)
        {
            NewProduct.PriceTotal = value ?? 0;
            CalcularPrecio();
            return Task.CompletedTask;
        }

        private void CalcularPrecio()
        {
            if (NewProduct.Amount > 0)
            {
                NewProduct.Price = Math.Round(
                    NewProduct.PriceTotal / NewProduct.Amount,
                    2,
                    MidpointRounding.AwayFromZero);
            }
            else
            {
                NewProduct.Price = 0;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await LoadProducts();
        }

        private async Task LoadProducts()
        {
            var responseHttp = await repository.GetAsync<List<Product>>("api/Products");
            Products = responseHttp.Response;
        }

        private void ShowCreateModal()
        {
            NewProduct = new Product();
            IsEditMode = false;
            IsModalVisible = true;
        }

        private void CloseModal()
        {
            IsModalVisible = false;
        }

        private void ShowEditModal(Product product)
        {
            // Clonar el objeto para no afectar la lista si cancelamos
            NewProduct = new Product
            {
                Id = product.Id,
                Name = product.Name,
                URLImagen = product.URLImagen,
                Description = product.Description,
                Amount = product.Amount,
                Price = product.Price,
                PriceTotal = product.PriceTotal,
                Items = product.Items
            };
            IsEditMode = true;
            IsModalVisible = true;
        }

        private async Task ConfirmDelete(Product product)
        {
            var result = await sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "¿Está seguro?",
                Text = $"Se eliminará el Producto '{product.Name}'. Esta acción no se puede deshacer.",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, borrar",
                CancelButtonText = "Cancelar"
            });

            if (!string.IsNullOrEmpty(result.Value))
            {
                await DeleteEventTypes(product);
            }
        }

        private async Task DeleteEventTypes(Product product)
        {
            var responseHttp = await repository.DeleteAsync<Product>($"api/Products/{product.Id}");

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar el Producto.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync("Eliminado", "El Producto fue borrado correctamente.", SweetAlertIcon.Success);

            await LoadProducts();
        }

        private async Task SaveProduct()
        {
            HttpResponseWrapper<object>? responseHttp;
            if (IsEditMode)
            {
                // PUT -> Editar
                responseHttp = await repository.PutAsync("api/Products", NewProduct);
            }
            else
            {
                // POST -> Crear
                responseHttp = await repository.PostAsync<Product>("api/Products", NewProduct);
            }

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar el Producto.";
                await sweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }

            // Cerrar el modal inmediatamente al confirmar que la operación fue exitosa
            CloseModal();

            // Luego mostrar la notificación
            var toast = sweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.TopEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true,
            });
            await toast.FireAsync(
                "Éxito",
                IsEditMode ? "Producto actualizado con éxito." : "Producto creado con éxito.",
                SweetAlertIcon.Success
            );

            await LoadProducts();
        }

        private void AddItem()
        {
            NewProduct.Items.Add(string.Empty);
        }

        private void RemoveItem(int index)
        {
            if (index >= 0 && index < NewProduct.Items.Count)
            {
                NewProduct.Items.RemoveAt(index);
            }
        }
    }
}