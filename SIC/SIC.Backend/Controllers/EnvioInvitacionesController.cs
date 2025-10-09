using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnvioInvitacionesController : GenericController<Event>
    {
        private readonly IWhatsAppConfigUnitOfWork _whatsAppConfigUnitOfWork;
        private readonly BoletaService _boletaService;
        private readonly WhatsAppService _whatsAppService;
        private readonly IEventsUnitOfWork _eventsUnitOfWork;
        private readonly IGenericUnitOfWork<InvitationSendLog> _logunitOfWork;
        private readonly IGenericUnitOfWork<MassiveShippingProgress> _MasiveShippingProgressUnitOfWork;
        private readonly IMessageUnitOfWork _messageUnitOfWork;

        public EnvioInvitacionesController(IGenericUnitOfWork<Event> unitOfWork, IGenericUnitOfWork<InvitationSendLog> logUnitOfWork, IGenericUnitOfWork<MassiveShippingProgress> MasiveShippingProgressUnitOfWork, IWhatsAppConfigUnitOfWork whatsAppConfigUnitOfWork, WhatsAppService whatsAppService, BoletaService boletaService, IEventsUnitOfWork eventsUnitOfWork, IMessageUnitOfWork messageUnitOfWork) : base(unitOfWork)
        {
            _whatsAppConfigUnitOfWork = whatsAppConfigUnitOfWork;
            _whatsAppService = whatsAppService;
            _boletaService = boletaService;
            _eventsUnitOfWork = eventsUnitOfWork;
            _logunitOfWork = logUnitOfWork;
            _messageUnitOfWork = messageUnitOfWork;
            _MasiveShippingProgressUnitOfWork = MasiveShippingProgressUnitOfWork;
        }

        //[HttpPost("enviar-masivo/{eventoId}")]
        //Logica para enviar masivo
        //Enviar en lotes de 10, primero va a consultar si tiene UsuarioWhatsAppConfigs activo buscando por el usuario
        //Buscar la lista de invitaciones del evento
        //Por cada invitacion, generar la boleta
        //Eviarr por whatsapp cada invitacion
        // ============================================================
        // 🔹 ENDPOINT 1: OBTENER EL PROGRESO DEL ENVÍO MASIVO
        // ============================================================
        [HttpGet("ultimo-punto/{code}")]
        public async Task<IActionResult> ObtenerUltimoPuntoEnvio(string code, [FromQuery] int tamanioTanda = 10)
        {
            var evento = await _eventsUnitOfWork.GetByCodeAsync(code);
            if (evento.Result == null)
                return NotFound("Evento no encontrado.");

            var invitaciones = evento.Result.Invitations
                .Where(i => !string.IsNullOrEmpty(i.PhoneNumber))
                .ToList();

            int totalInvitaciones = invitaciones.Count;
            int totalEnviadas = await _logunitOfWork.CountAsync(l =>
                l.Invitation!.EventId == evento.Result.Id && l.IsSuccessful);

            int totalTandas = (int)Math.Ceiling((double)totalInvitaciones / tamanioTanda);
            int ultimaTanda = totalEnviadas == 0 ? 0 : (int)Math.Ceiling((double)totalEnviadas / tamanioTanda);
            bool completado = totalEnviadas >= totalInvitaciones;
            int? siguienteTanda = completado ? null : ultimaTanda + 1;

            return Ok(new
            {
                Evento = evento.Result.Name,
                TotalInvitaciones = totalInvitaciones,
                TotalEnviadas = totalEnviadas,
                UltimaTanda = ultimaTanda,
                SiguienteTanda = siguienteTanda,
                TotalTandas = totalTandas,
                Completado = completado
            });
        }

        [HttpPost("enviar-masivo/{code}")]
        public async Task<IActionResult> EnviarMasivo(string code, [FromQuery] int? tanda = null, [FromQuery] int tamanioTanda = 10, bool isInvitationMessage = true)
        {
            // 🔐 ID del usuario autenticado (en tu caso, temporalmente hardcodeado)
            var usuarioId = "b5e6b942-98b4-4585-8e1a-1ec478e388cf";

            var configWhatsApp = await _whatsAppConfigUnitOfWork.GetByUserIdAsync(usuarioId);
            if (configWhatsApp == null)
                return BadRequest("El usuario no tiene configuradas sus credenciales de WhatsApp.");

            var evento = await _eventsUnitOfWork.GetByCodeAsync(code);
            if (evento == null || evento.Result == null)
                return NotFound("Evento no encontrado.");

            var mensaje = await _messageUnitOfWork.GetByCodeAsync(code);
            if (mensaje.Result == null)
                return BadRequest("No se encontró el mensaje para el evento.");

            // 🔹 Buscar progreso anterior
            var progreso = await _whatsAppConfigUnitOfWork.GetByEventoUserAsync(evento.Result.Id);

            var totalInvitaciones = evento.Result.Invitations
                .Where(i => !string.IsNullOrEmpty(i.PhoneNumber))
                .Count();

            var totalTandas = (int)Math.Ceiling((double)totalInvitaciones / tamanioTanda);

            // 🔹 Determinar tanda actual
            int tandaActual = tanda ?? (progreso.Result?.UltimaTandaEnviada + 1 ?? 1);

            if (tandaActual > totalTandas)
                return Ok(new { mensaje = "Todas las tandas ya fueron enviadas.", completado = true });

            var invitaciones = evento.Result.Invitations
                .Where(i => !string.IsNullOrEmpty(i.PhoneNumber))
                .Skip((tandaActual - 1) * tamanioTanda)
                .Take(tamanioTanda)
                .ToList();

            if (!invitaciones.Any())
                return Ok("No hay más invitaciones pendientes en esta tanda.");

            int enviadas = 0, fallidas = 0;

            foreach (var invitacion in invitaciones)
            {
                var dto = new BoletaInvitacionDto
                {
                    NombreInvitado = invitacion.Name,
                    NombreEvento = evento.Result.Name,
                    Fecha = evento.Result.Date,
                    Hora = evento.Result.Time.ToString(@"hh\:mm"),
                    Lugar = evento.Result.Url,
                    CantidadPersonas = invitacion.NumberAdults + invitacion.NumberChildren,
                    MesaAsignada = invitacion.Table ?? "Sin asignar",
                    CodigoQr = invitacion.Code ?? $"INV-{invitacion.Id}-{evento.Result.Id}"
                };

                var (pdfBytes, _) = _boletaService.GenerarBoleta(dto);
                var resultado = await _whatsAppService.enviaAsync(configWhatsApp.Result!.AccessToken);
                /*var resultado = await _whatsAppService.EnviarTextoAsync(
                    configWhatsApp.Result!.AccessToken,
                    configWhatsApp.Result!.PhoneNumberId,
                    invitacion.PhoneNumber,
                    mensaje.Result!.MessageInvitation
                );
                var resultado = await _whatsAppService.EnviarInvitacionAsync(
                    configWhatsApp.Result!.AccessToken,
                    configWhatsApp.Result!.PhoneNumberId,
                    invitacion.PhoneNumber,
                    mensaje.Result!.MessageInvitation,
                    pdfBytes,
                    $"Boleta_{invitacion.Name}.pdf"
                );*/

                await _logunitOfWork.AddAsync(new InvitationSendLog
                {
                    InvitationId = invitacion.Id,
                    SendDate = DateTime.Now,
                    IsSuccessful = resultado.success,
                    WhatsAppMessageId = resultado.messageId,
                    ErrorMessage = resultado.error,
                    AttemptNumber = progreso.Result?.UltimaTandaEnviada + 1 ?? 1
                });

                if (resultado.success) enviadas++; else fallidas++;

                await Task.Delay(2000);
            }

            // 🔹 Actualizar progreso
            if (progreso.Result == null)
            {
                await _MasiveShippingProgressUnitOfWork.AddAsync(new MassiveShippingProgress
                {
                    EventoId = evento.Result.Id,
                    UltimaTandaEnviada = tandaActual,
                    TotalTandas = totalTandas,
                    FechaUltimoEnvio = DateTime.Now,
                    Completado = tandaActual >= totalTandas
                });
            }
            else
            {
                progreso.Result.UltimaTandaEnviada = tandaActual;
                progreso.Result.FechaUltimoEnvio = DateTime.Now;
                progreso.Result.Completado = tandaActual >= totalTandas;
                await _MasiveShippingProgressUnitOfWork.UpdateAsync(progreso.Result);
            }

            return Ok(new
            {
                Evento = evento.Result.Name,
                Enviadas = enviadas,
                Fallidas = fallidas,
                TotalProcesadas = enviadas + fallidas,
                TandaActual = tandaActual,
                SiguienteTanda = tandaActual < totalTandas ? (int?)tandaActual + 1 : null,
                TotalTandas = totalTandas,
                Completado = tandaActual >= totalTandas
            });
        }

        //_whatsAppConfigUnitOfWork
        [HttpPost("configurar")]
        public async Task<IActionResult> ConfigurarWhatsApp([FromBody] UsuarioWhatsAppConfig usuarioWhatsAppConfig)
        {
            var action = await _whatsAppConfigUnitOfWork.AddFullAsync(usuarioWhatsAppConfig);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }

        //_whatsAppConfigUnitOfWork
        [HttpPut("configurar")]
        public async Task<IActionResult> ConfigurarWhatsAppPut([FromBody] UsuarioWhatsAppConfig usuarioWhatsAppConfig)
        {
            var action = await _whatsAppConfigUnitOfWork.UpdateFullAsync(usuarioWhatsAppConfig);
            if (action.Success)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }
    }
}