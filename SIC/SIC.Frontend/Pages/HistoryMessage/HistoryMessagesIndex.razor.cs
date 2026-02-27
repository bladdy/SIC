using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

//ToDo: agregar filtros y paginacion
namespace SIC.Frontend.Pages.HistoryMessage
{
    public partial class HistoryMessagesIndex
    {
        [Inject] private IRepository Repository { get; set; } = default!;

        public List<HistoryMessages>? HistoryMessages { get; set; } = [];
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadHistoryMessages();
        }

        private async Task LoadHistoryMessages()
        {
            var url = $"api/messages/HistoryMessages";

            var responseHttp = await Repository.GetAsync<List<HistoryMessages>>(url);

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                }
            }

            HistoryMessages = responseHttp?.Response ?? new List<HistoryMessages>();
        }
    }
}