using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.MessagesRecive
{
    public partial class GetAllMessagesReciveIndex
    {
        [Inject] private IRepository Repository { get; set; } = default!;

        public List<MessagesReciveDTO>? MessagesRecive { get; set; } = [];
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await LoadHistoryMessages();
        }

        private async Task LoadHistoryMessages()
        {
            var url = $"api/messages/GetAllMessagesRecive";

            var responseHttp = await Repository.GetAsync<List<MessagesReciveDTO>>(url);

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/");
                    var message = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                }
            }

            MessagesRecive = responseHttp?.Response ?? new List<MessagesReciveDTO>();
        }
    }
}