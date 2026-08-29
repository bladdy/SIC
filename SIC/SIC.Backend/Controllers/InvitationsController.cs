using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using SIC.Shared.Helpers;

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

    [HttpGet("byEventCode/{code}")]
    public async Task<IActionResult> GetByEventCodeAsync(string code)
    {
        var response = await _invitationUnitOfWork.GetAllAsync(code);
        if (response.Success)
        {
            return Ok(response.Result);
        }
        return NotFound();
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

    [HttpDelete("{id}")]
    public override async Task<IActionResult> Delete(int id)
    {
        var action = await _invitationUnitOfWork.DeleteByIdAsync(id);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

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

    [HttpPut("update-invitation")]
    public async Task<IActionResult> UpdateInvitation(ResponseInvitationDTO invitation)
    {
        var action = await _invitationUnitOfWork.UpdateForConfirmationListFullAsync(invitation);
        if (action.Success)
        {
            return Ok(action.Result);
        }
        return NotFound(action.Message);
    }

    [HttpGet("generatedpdf")]
    public async Task<IActionResult> GetLitPdf(string evento)
    {
        try
        {
            var response = await _invitationUnitOfWork.GetAllAsync(evento);
            var invitaciones = response.Result?.ToList();

            if (invitaciones == null || !invitaciones.Any())
                return NotFound("No hay invitados.");

            var pdfBytes = await _boletaService.GenerarListaPdf(invitaciones);

            return File(
                pdfBytes,
                "application/pdf",
                $"lista-invitados-{evento}.pdf"
            );
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("qr")]
    public async Task<IActionResult> GetQRCodeAsync(string codigo, string evento)
    {
        try
        {
            var response = await _invitationUnitOfWork.GetByCodeAsync(codigo);
            var invitacion = response.Result;
            if (invitacion == null)
                return NotFound(new { success = false, message = "Invitación no encontrada." });

            Console.WriteLine($"[Boleta] invitacion.TablesEventsId={invitacion.TablesEventsId}, TablesEvents?.Name={invitacion.TablesEvents?.Name}");
            Console.WriteLine($"[Boleta] Guests count={invitacion.Guests?.Count}");
            foreach (var g in invitacion.Guests ?? Enumerable.Empty<SIC.Shared.Entities.InvitationGuest>())
            {
                Console.WriteLine($"[Boleta] Guest '{g.GuestName}' TablesEventsId={g.TablesEventsId}, TablesEvents?.Name={g.TablesEvents?.Name}");
            }

            var dto = new BoletaInvitacionDto
            {
                NombreInvitado = invitacion.Name,
                NombreEvento = invitacion.Event!.Name,
                SubNombre = invitacion.Event!.SubTitle,
                CoverImageBytes = invitacion.Event!.CoverImageUrl!,
                Fecha = invitacion.Event!.Date,
                Hora = DateTime.Today.Add(invitacion.Event!.Time).ToString("hh:mm tt"),
                Lugar = invitacion.Event!.Url!,
                CantidadPersonas = invitacion.NumberAdults + invitacion.NumberChildren,
                Guests = invitacion.Guests.Where(s => s.Status == Status.Attend).Select(c => $"{c.GuestName} ({c.GuestType.GetDescription()})").ToList(),
                Niños = invitacion.NumberConfirmedChildren,
                Jovenes = invitacion.NumberConfirmedYouths,
                Adultos = invitacion.NumberConfirmedAdults,
                MesaAsignada = invitacion.TablesEvents?.Name
                    ?? invitacion.Guests?.FirstOrDefault(g => g.TablesEventsId.HasValue)?.TablesEvents?.Name
                    ?? "Sin asignar",
                CodigoQr = invitacion.Code ?? $"INV-{invitacion.Id}-{evento}",
                IsIndividualAssignment = invitacion.TablesEventsId == null
                    && invitacion.Guests.Any(g => g.TablesEventsId.HasValue),
                GuestsWithMesa = invitacion.TablesEventsId == null
                    && invitacion.Guests.Any(g => g.TablesEventsId.HasValue)
                    ? invitacion.Guests
                        .Where(g => g.Status == Status.Attend)
                        .Select(g => $"{g.GuestName} ({g.GuestType.GetDescription()}) - Mesa: {g.TablesEvents?.Name ?? "Sin asignar"}")
                        .ToList()
                    : []
            };

            // 🔹 QR Base64
            string qrBase64 = GenerateQRCodeBase64(dto.CodigoQr, evento);
            byte[] qrBytes = Convert.FromBase64String(qrBase64);
            // 🔹 PDF Base64
            //var (pdfBytes, _) = _boletaService.GenerarBoleta(dto);
            var pdfQRByte = _boletaService.GenerarBoletaEstiloCard(dto, qrBytes);
            //string pdfBase64 = Convert.ToBase64String(pdfBytes);
            string pdfBase64 = Convert.ToBase64String(await pdfQRByte);

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