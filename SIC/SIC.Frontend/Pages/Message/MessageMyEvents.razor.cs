using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.Message
{
    public partial class MessageMyEvents
    {
        [Parameter] public string? Code { get; set; }
        public Event? EventDetail { get; set; }
        private bool isEditOrCreate = false;
        private bool isLoading = false;
        public SIC.Shared.Entities.Message? NewMessage { get; set; }
        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;

        private string Title = "No";
        private string SubTitle = "No";

        public List<MessageKey>? Tokens { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadEvent();
            await LoadMessage();
            await LoadKeys();
        }

        private async Task LoadKeys()
        {
            var responseHttp = await Repository.GetAsync<List<MessageKey>>($"api/Messages/Keys");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/my-events");
                    return;
                }
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            Tokens = responseHttp?.Response ?? new List<MessageKey>();
        }

        private async Task LoadEvent()
        {
            var responseHttp = await Repository.GetAsync<Event>($"api/Events/byCode/{Code}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/my-events");
                    return;
                }
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            EventDetail = responseHttp?.Response;
        }

        private async Task LoadMessage()
        {
            var responseHttp = await Repository.GetAsync<SIC.Shared.Entities.Message>($"api/Messages/byCode/{Code}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/my-events");
                    return;
                }
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            if (responseHttp?.HttpResponseMessage.StatusCode == HttpStatusCode.NoContent)
            {
                NewMessage = new SIC.Shared.Entities.Message
                {
                    Title = EventDetail!.Name,
                    SubTitle = EventDetail.SubTitle,
                    MessageInvitation = $"¡Hola! Estás cordialmente invitado a {{EventDetail.Name}} que se celebrará el {EventDetail.Date:dddd, dd 'de' MMMM 'de' yyyy} a las {EventDetail.Date:hh:mm tt}. Por favor, confirma tu asistencia utilizando el siguiente enlace: {{linkinvitacion}}. Esperamos contar con tu presencia para compartir este momento especial.",
                    MessageConfirmation = $"¡Gracias por confirmar tu asistencia a {EventDetail.Name}! Nos alegra saber que podrás acompañarnos en este día tan especial. Si tienes alguna pregunta o necesitas más información, no dudes en contactarnos. ¡Nos vemos pronto!"
                };
                isEditOrCreate = true;
            }
            else
            {
                NewMessage = responseHttp?.Response;
                isEditOrCreate = false;
            }
        }

        private async Task HandleValidSubmit()
        {
            HttpResponseWrapper<object>? responseHttp;
            isLoading = true;

            if (isEditOrCreate)
            {
                // POST -> Crear https://localhost:7141/api/Messages/full?code=qeqweqwe
                responseHttp = await Repository.PostAsync($"api/Messages/full?code={Code}", NewMessage);
            }
            else
            {
                // PUT -> Editar
                responseHttp = await Repository.PutAsync($"api/Messages/full?code={Code}", NewMessage);
            }

            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync() ?? "No se pudo guardar Los mensajes.";
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
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
                isEditOrCreate ? "Los mensajes han sido creados con éxito." : "Los mensajes han sido actualizados con éxito.",
                SweetAlertIcon.Success
            );
            NavigationManager.NavigateTo($"/my-events/details/{Code}");
            isLoading = false;
        }

        private void BackToEvent()
        {
            NavigationManager.NavigateTo($"/my-events/details/{Code}");
        }
    }
}