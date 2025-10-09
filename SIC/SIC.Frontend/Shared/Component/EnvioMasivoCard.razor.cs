using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.DTOs;
using System;
using System.Threading.Tasks;

namespace SIC.Frontend.Shared.Component
{
    public partial class EnvioMasivoCard
    {
        [Inject] private IRepository Repository { get; set; } = default!;
        [Parameter] public string? Code { get; set; }

        private ProgresoEnvioDto? progreso;
        private bool isLoading = false;
        private bool completado = false;
        private string? mensaje;

        protected override async Task OnInitializedAsync()
        {
            await CargarProgreso();
        }

        private async Task CargarProgreso()
        {
            if (string.IsNullOrEmpty(Code)) return;

            var response = await Repository.GetAsync<ProgresoEnvioDto>($"api/envioinvitaciones/ultimo-punto/{Code}");
            progreso = response.Response;
            completado = progreso?.Completado ?? false;
            StateHasChanged();
        }

        private async Task EnviarSiguienteTanda()
        {
            if (string.IsNullOrEmpty(Code) || progreso == null || progreso.Completado)
                return;

            isLoading = true;
            mensaje = $"Enviando tanda {progreso.SiguienteTanda ?? progreso.UltimaTanda + 1}...";
            StateHasChanged();

            try
            {
                // Bucle secuencial: sigue enviando tandas hasta que se complete
                do
                {
                    var tanda = progreso.SiguienteTanda ?? (progreso.UltimaTanda + 1);

                    // Endpoint: api/envioinvitaciones/enviar-masivo/{code}?tanda=X
                    var response = await Repository.PostAsync<object, ProgresoEnvioDto>(
                        $"api/envioinvitaciones/enviar-masivo/{Code}?tanda={tanda}", new { });

                    if (!response.Response!.Completado)
                    {
                        mensaje = "Error al enviar la tanda.";
                        break;
                    }

                    // Refresca progreso actual
                    await CargarProgreso();

                    if (progreso!.Completado)
                    {
                        completado = true;
                        mensaje = "?? Todas las tandas fueron enviadas correctamente.";
                        break;
                    }

                    // Pequeña pausa opcional entre tandas
                    await Task.Delay(2000);
                } while (!(progreso?.Completado ?? true));
            }
            catch (Exception ex)
            {
                mensaje = $"? Error: {ex.Message}";
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
    }
}