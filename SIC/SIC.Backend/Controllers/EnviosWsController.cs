using Microsoft.AspNetCore.Mvc;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers
{
    public class EnviosWsController : ControllerBase
    {
        private readonly IMessageUnitOfWork _messageUnitOfWork;

        public EnviosWsController(IMessageUnitOfWork messageUnitOfWork)
        {
            _messageUnitOfWork = messageUnitOfWork;
        }

        [HttpGet("ListaEnvio/{eventoId}")]
        public async Task<IActionResult> ObtenerListaEnvioWhatsApp(string eventoId)
        {
            try
            {
                // obtener la lista de invitaciones con los mensajes por evento
                var response = await _messageUnitOfWork.GetMessageWhatsappInvitation(eventoId);
                if (response.Success)
                {
                    return Ok(response.Result);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, detalle = ex.StackTrace });
            }
        }
    }
}