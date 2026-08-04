using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Protocol;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Response;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace SIC.Frontend.Pages.Whatsapp
{
    public partial class TemplateIndex
    {
        private bool isGenerating;
        [Inject] private IRepository Repository { get; set; } = default!;

        public WhatsappTemplates? WhatsappTemplates { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        private static readonly HashSet<string> TemplateImages = new(StringComparer.OrdinalIgnoreCase)
        {
            "recordatorio_registro_de_asistentes",
            "cierre_listas_de_asistencia",
            "confirmacion_de_acceso_con_mesa",
            "confirmacion_de_acceso_general",
            "registro_de_asistentes",
            "save_the_date"
        };

        private string? SelectedImageUrl;
        private string? SelectedImageName;

        private string? GetTemplateImageUrl(string? templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName)) return null;

            var name = templateName.Trim().ToLower();
            if (TemplateImages.Contains(name))
                return $"{NavigationManager.BaseUri}template-img/{name}.png";

            var match = TemplateImages.FirstOrDefault(img => name.StartsWith(img));
            return match == null
                ? null
                : $"{NavigationManager.BaseUri}template-img/{match}.png";
        }

        private async Task OpenTemplateImage(TemplateDatum template)
        {
            var url = GetTemplateImageUrl(template.Name);
            if (url == null)
            {
                await SweetAlertService.FireAsync("Sin imagen",
                    $"No hay imagen disponible para la plantilla '{template.Name}'.",
                    SweetAlertIcon.Info);
                return;
            }

            SelectedImageUrl = url;
            SelectedImageName = template.Name;
        }

        private void CloseTemplateImage()
        {
            SelectedImageUrl = null;
            SelectedImageName = null;
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadAllTemplates();
        }

        private async Task LoadAllTemplates()
        {
            var url = $"api/whatsapp/chat/templates";

            var responseHttp = await Repository.GetAsync<WhatsappTemplates>(url);

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound || responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    NavigationManager.NavigateTo("/");
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                }
            }

            WhatsappTemplates = responseHttp?.Response ?? new WhatsappTemplates();
        }

        private async Task GenerateTemplates()
        {
            HttpResponseWrapper<object>? responseHttp;
            var confirm = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Generar plantillas",
                Text = "Se crear�n las 5 plantillas en segundo plano. El proceso puede tardar aproximadamente 5 minutos.",
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true,
                ConfirmButtonText = "S�, generar",
                CancelButtonText = "Cancelar"
            });

            if (!confirm.IsConfirmed)
                return;

            isGenerating = true;

            try
            {
                responseHttp = await Repository.PostAsync<object>("api/whatsapp/generate-templates");

                if (responseHttp.Error)
                {
                    var error = await responseHttp.GetErrorMessageAsync() ?? "No se pudo Generar las plantillas.";

                    await SweetAlertService.FireAsync(new SweetAlertOptions
                    {
                        Title = "Error",
                        Text = error,
                        Icon = SweetAlertIcon.Error
                    });
                }
                else
                {
                    // Cast the response to HttpResponseMessage to access Content
                    var httpResponseMessage = responseHttp.HttpResponseMessage;
                    var result = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse>();

                    var message = result?.Message;
                    await SweetAlertService.FireAsync(new SweetAlertOptions
                    {
                        Title = "Generar plantillas sugeridas",
                        Text = message,
                        Icon = SweetAlertIcon.Success
                    });
                }
            }
            catch (Exception ex)
            {
                await SweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Title = "Error",
                    Text = ex.Message,
                    Icon = SweetAlertIcon.Error
                });
            }
            finally
            {
                isGenerating = false;
            }
        }
    }
}