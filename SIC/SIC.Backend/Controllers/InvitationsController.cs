using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationsController : GenericController<Invitation>
{
    private readonly IInvitationUnitOfWork _invitationUnitOfWork;
    private readonly BoletaService _boletaService;

    public InvitationsController(IGenericUnitOfWork<Invitation> unitOfWork, IInvitationUnitOfWork invitationUnitOfWork, BoletaService boletaService) : base(unitOfWork)
    {
        _invitationUnitOfWork = invitationUnitOfWork;
        _boletaService = boletaService;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAsync()
    {
        var response = await _invitationUnitOfWork.GetAsync();
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("paginated")]
    public override async Task<IActionResult> GetAsync(PaginationDTO pagination)
    {
        var response = await _invitationUnitOfWork.GetAsync(pagination);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpGet("totalRecords")]
    public override async Task<IActionResult> GetTotalRecordsAsync([FromQuery] PaginationDTO pagination)
    {
        var action = await _invitationUnitOfWork.GetTotalRecordAsync(pagination);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return BadRequest();
    }

    [HttpGet("byCode/{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        var response = await _invitationUnitOfWork.GetByCodeAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
    }

    [HttpPost("full")]
    public async Task<IActionResult> PostFullAsync(Invitation invitation)
    {
        var action = await _invitationUnitOfWork.AddFullAsync(invitation);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpPut("full")]
    public async Task<IActionResult> PutFullAsync(Invitation invitation)
    {
        var action = await _invitationUnitOfWork.UpdateFullAsync(invitation);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    /*
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var action = await _invitationUnitOfWork.DeleteByIdAsync(id);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }
    */

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmInvitation([FromBody] InvitationConfirmationDto confirmation)
    {
        var action = await _invitationUnitOfWork.UpdateForConfirmarionFullAsync(confirmation);
        if (action.Success)
        {
            return Ok(action.Result);
        }

        return Ok(new { message = "Confirmación recibida con éxito" });
    }

    // Endpoint para obtener el código QR
    // ToDo: Cambiar el codigo para que genere la boleta con lo datos del evento, invitados y el Qr
    [HttpGet("qr")]
    public async Task<IActionResult> GetQRCodeAsync(string codigo, string evento)
    {
        try
        {
            var response = await _invitationUnitOfWork.GetByCodeAsync(codigo);
            var invitacion = response.Result;
            if (invitacion == null)
                return NotFound(new { success = false, message = "Invitación no encontrada." });

            var dto = new BoletaInvitacionDto
            {
                NombreInvitado = invitacion.Name,
                NombreEvento = invitacion.Event!.Name,
                SubNombre = invitacion.Event!.SubTitle,
                Fecha = invitacion.Event!.Date,
                Hora = DateTime.Today.Add(invitacion.Event!.Time).ToString("hh:mm tt"),
                Lugar = invitacion.Event!.Url!,
                CantidadPersonas = invitacion.NumberAdults + invitacion.NumberChildren,
                Niños = invitacion.NumberConfirmedChildren,
                Adultos = invitacion.NumberConfirmedAdults,
                MesaAsignada = invitacion.Table ?? "Sin asignar",
                CodigoQr = invitacion.Code ?? $"INV-{invitacion.Id}-{evento}"
            };

            // 🔹 QR Base64
            string qrBase64 = GenerateQRCodeBase64(dto.CodigoQr, evento);

            // 🔹 PDF Base64
            var (pdfBytes, _) = _boletaService.GenerarBoleta(dto);
            string pdfBase64 = Convert.ToBase64String(pdfBytes);

            return Ok(new
            {
                success = true,
                qrBase64,
                pdfBase64
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    private static string GenerateQRCodeBase64(string codigo, string evento)
    {
        // Aquí puedes usar alguna librería como QRCoder para generar el QR en base64
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode($"{codigo}", QRCodeGenerator.ECCLevel.Q);
        var qrCode = new Base64QRCode(qrCodeData);
        return qrCode.GetGraphic(20);
    }
}