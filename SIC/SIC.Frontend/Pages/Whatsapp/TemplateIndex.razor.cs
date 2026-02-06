using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using System.Net;

namespace SIC.Frontend.Pages.Whatsapp
{
    public partial class TemplateIndex
    {
        [Inject] private IRepository Repository { get; set; } = default!;

        public WhatsappTemplates? WhatsappTemplates { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

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
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                }
            }

            WhatsappTemplates = responseHttp?.Response ?? new WhatsappTemplates();
        }
    }
}