using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.RegisterEvent
{
    public partial class RegisterIntationsByEvent
    {
        [Parameter] public string? Code { get; set; }
        public Event? EventDetail { get; set; }
        [Inject] private IJSRuntime JS { get; set; } = default!;

        [Inject] private IRepository Repository { get; set; } = default!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
        [Inject] private NavigationManager NavigationManager { get; set; } = default!;
        private DotNetObjectReference<object>? objRef;
        private bool isScannerRunning = false;
        private string? qrResult;

        //Modal
        private bool showModal = false;

        private int adultos = 0;
        private int ninos = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadEvent();
        }

        private void OpenModal()
        {
            adultos = 0;
            ninos = 0;
            showModal = true;
            StateHasChanged();
        }

        private void CloseModal()
        {
            showModal = false;
            StateHasChanged();
        }

        private void CancelModal()
        {
            showModal = false;
            StateHasChanged();
        }

        private async Task StartScannerAsync()
        {
            if (isScannerRunning) return;

            objRef?.Dispose(); // limpiar referencia anterior
            objRef = DotNetObjectReference.Create<object>(this);

            await JS.InvokeVoidAsync("qrScanner.start", objRef);
            isScannerRunning = true;
        }

        private async Task SubmitModal()
        {
            showModal = false;

            // Aquí puedes registrar los datos de adultos y niños
            //await RegistrarAsistentes(qrResult, adultos, ninos);

            // Luego muestras el SweetAlert
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Registro de invitacion",
                Text = qrResult,
                Icon = SweetAlertIcon.Success,
                ShowCancelButton = true,
                ConfirmButtonText = "Escanear otro QR",
                CancelButtonText = "Finalizar",
                ReverseButtons = true
            });

            if (result.IsConfirmed)
            {
                await StartScannerAsync();
            }
        }

        [JSInvokable]
        public async Task OnQrCodeScanned(string code)
        {
            qrResult = code;

            await JS.InvokeVoidAsync("qrScanner.stop");
            isScannerRunning = false;
            StateHasChanged();

            OpenModal(); // <-- Abre el modal para registrar adultos y niños
        }

        private async Task LoadEvent()
        {
            var responseHttp = await Repository.GetAsync<Event>($"api/Events/byCode/{Code}");
            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/events");
                    return;
                }
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            EventDetail = responseHttp.Response;
        }
    }
}