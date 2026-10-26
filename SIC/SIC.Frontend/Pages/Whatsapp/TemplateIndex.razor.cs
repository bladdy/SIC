using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Protocol;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Response;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace SIC.Frontend.Pages.Whatsapp
{
    public partial class TemplateIndex
    {
        private bool isGenerating;
        private bool hasGeneratedSuggested;
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

        private TemplateDatum? SelectedTemplate;

        private void OpenTemplatePreview(TemplateDatum template)
        {
            SelectedTemplate = template;
        }

        private void CloseTemplatePreview()
        {
            SelectedTemplate = null;
        }

        private bool HasImage(TemplateDatum template)
        {
            return template.Components?.Any(c =>
                string.Equals(c.Type, "HEADER", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Format, "IMAGE", StringComparison.OrdinalIgnoreCase)) == true;
        }

        private bool HasButtons(TemplateDatum template)
        {
            return template.Components?.Any(c =>
                string.Equals(c.Type, "BUTTONS", StringComparison.OrdinalIgnoreCase)) == true;
        }

        private string GetHeaderImageUrl(TemplateDatum template)
        {
            var header = template.Components?.FirstOrDefault(c =>
                string.Equals(c.Type, "HEADER", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Format, "IMAGE", StringComparison.OrdinalIgnoreCase));

            return header?.Example?.HeaderHandle?.FirstOrDefault() ?? string.Empty;
        }

        private MarkupString RenderBodyText(Component component)
        {
            var text = component.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(text))
                return new MarkupString("");

            var bodyText = component.Example?.BodyText?.FirstOrDefault();
            if (bodyText != null)
            {
                for (int i = 0; i < bodyText.Count; i++)
                {
                    text = text.Replace($"{{{{{i + 1}}}}}", bodyText[i] ?? string.Empty);
                }
            }

            text = text.Replace("\n", "<br>");
            // Convertir *texto* en negritas
            text = Regex.Replace(text, @"\*(.*?)\*", "<strong>$1</strong>");
            return new MarkupString(text);
        }

        private async Task DeleteTemplate(TemplateDatum template)
        {
            _pendingTemplate = template;
            ConfirmMessage = $"¿Estás seguro de que deseas eliminar la plantilla '{template.Name}'? " +
                             "Tendrás que esperar 30 días antes de poder reutilizar el mismo nombre para crear una nueva plantilla.";
            IsConfirmVisible = true;
            await Task.CompletedTask;
        }

        private bool IsConfirmVisible;
        private string ConfirmMessage = "";
        private TemplateDatum? _pendingTemplate;

        private async Task OnConfirmVisibleChanged(bool visible)
        {
            IsConfirmVisible = visible;
            if (!visible) _pendingTemplate = null;
        }

        private async Task ExecutePendingDelete()
        {
            if (_pendingTemplate == null) return;
            var template = _pendingTemplate;
            _pendingTemplate = null;

            var url = $"api/whatsapp/chat/templates/{Uri.EscapeDataString(template.Name!)}";

            var responseHttp = await Repository.DeleteAsync<object>(url);

            if (responseHttp.Error)
            {
                var error = await responseHttp.GetErrorMessageAsync() ?? "No se pudo eliminar la plantilla.";
                await SweetAlertService.FireAsync("Error", error, SweetAlertIcon.Error);
                return;
            }

            var httpResponseMessage = responseHttp.HttpResponseMessage;
            var result = await httpResponseMessage.Content.ReadFromJsonAsync<ApiResponse>();

            await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Plantilla eliminada",
                Text = result?.Message ?? $"La plantilla '{template.Name}' fue eliminada correctamente.",
                Icon = SweetAlertIcon.Success
            });

            await LoadAllTemplates();
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadAllTemplates();
            await LoadHasGeneratedTemplates();
        }

        private async Task LoadHasGeneratedTemplates()
        {
            var responseHttp = await Repository.GetAsync<HasGeneratedTemplatesDto>($"api/whatsapp/has-generated-templates");

            hasGeneratedSuggested = !responseHttp.Error && responseHttp.Response?.HasGenerated == true;
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
                Text = "Se crearán las 6 plantillas en segundo plano. El proceso puede tardar aproximadamente 6 minutos.",
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true,
                ConfirmButtonText = "Sí, generar",
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
                    // El proceso corre en segundo plano; se deshabilita el botón
                    // para evitar generar de nuevo mientras tanto.
                    hasGeneratedSuggested = true;

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