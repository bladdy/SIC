using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Response;
using System.Net.Http.Json;

namespace SIC.Frontend.Pages.Confirmations
{
    public partial class InvitationResponse : ComponentBase
    {
        [Parameter] public string? Code { get; set; }

        [Inject] public HttpClient Http { get; set; } = default!;
        [Inject] private IRepository repository { get; set; } = default!;
        [Inject] private SweetAlertService sweetAlertService { get; set; } = default!;

        protected Invitation? Invitacion;

        protected bool MostrarFormulario;
        protected bool MostrarGracias;
        protected bool MostrarNoInvitacion;
        protected bool MostrarListadoInvitados;
        protected bool MostrarBotonConfirmar;

        private bool? _asistira;

        protected bool? Asistira
        {
            get => _asistira;
            set
            {
                if (value == null || Invitacion == null)
                    return;

                _asistira = value;

                Invitacion.Status = value.Value
                    ? Status.Attend
                    : Status.NotAttend;

                foreach (var g in Invitacion.Guests)
                    g.Status = Invitacion.Status;

                MostrarListadoInvitados = true;   // 👈 SIEMPRE visible
                MostrarBotonConfirmar = true;
            }
        }


        protected string? CodigoNoEncontrado;
        protected string? QrBase64;
        protected string? PdfBase64;
        protected string? Comentarios;

        protected int Adultos;
        protected int Jovenes;
        protected int Menores;

        protected override async Task OnInitializedAsync()
        {
            if (string.IsNullOrEmpty(Code))
            {
                MostrarNoInvitacion = true;
                CodigoNoEncontrado = "SIN CÓDIGO";
                return;
            }

            await CargarInvitacion(Code);
        }

        private async Task CargarInvitacion(string code)
        {
            try
            {
                var response = await repository.GetAsync<Invitation>(
                    $"api/Invitations/byCode/{code}");

                if (response.Error || response.Response == null)
                {
                    MostrarNoInvitacion = true;
                    CodigoNoEncontrado = code;
                    return;
                }

                Invitacion = response.Response;

                if (Invitacion.Status == Status.Pending)
                {
                    MostrarFormulario = true;
                    MostrarListadoInvitados = true;   // 👈 IMPORTANTE
                    MostrarBotonConfirmar = false;    // 👈 opcional (se activará al elegir)
                    ContarInvitados();
                }
                else
                {
                    MostrarGracias = true;
                    await CargarQr();
                }
            }
            catch
            {
                MostrarNoInvitacion = true;
                CodigoNoEncontrado = code;
            }
        }

        private void ContarInvitados()
        {
            Adultos = Invitacion!.Guests.Count(g => g.GuestType == GuestType.Adult);
            Jovenes = Invitacion.Guests.Count(g => g.GuestType == GuestType.Youth);
            Menores = Invitacion.Guests.Count(g => g.GuestType == GuestType.Children);
        }

        protected async Task EnviarRespuesta()
        {
            if (Invitacion == null)
                return;

            var dto = new ResponseInvitationDTO
            {
                Code = Invitacion.Code!,
                Status = (int)Invitacion.Status,
                Comments = Comentarios,
                Guests = Invitacion.Guests
                    .Select(g => new GuestDTO
                    {
                        Id = g.Id,
                        GuestName = g.GuestName,
                        GuestType = (int)g.GuestType,
                        InvitationId = g.InvitationId,
                        Status = (int)g.Status
                    })
                    .ToList()
            };

            var response = await Http.PutAsJsonAsync(
                "api/Invitations/update-invitation",
                dto);

            if (response.IsSuccessStatusCode)
            {
                MostrarFormulario = false;
                MostrarGracias = true;

                if (Invitacion.Status == Status.Attend)
                    await CargarQr();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                await sweetAlertService.FireAsync("Error", error, SweetAlertIcon.Error);
            }
        }

        private async Task CargarQr()
        {
            try
            {
                var result = await Http.GetFromJsonAsync<QrApiResponse>(
                    $"api/Invitations/qr?codigo={Invitacion!.Code}&evento={Invitacion!.Event!.Code}");

                if (result != null && result.Success)
                {
                    QrBase64 = result.QrBase64;
                    PdfBase64 = result.PdfBase64;
                }
                else
                {
                    await sweetAlertService.FireAsync(
                        "Error",
                        result?.Message ?? "Error al generar el código QR.",
                        SweetAlertIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando QR: {ex.Message}");
            }
        }
    }
}