using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using QRCoder;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvitationsController : GenericController<Invitation>
{
    private readonly IInvitationUnitOfWork _invitationUnitOfWork;

    public InvitationsController(IGenericUnitOfWork<Invitation> unitOfWork, IInvitationUnitOfWork invitationUnitOfWork) : base(unitOfWork)
    {
        _invitationUnitOfWork = invitationUnitOfWork;
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
    [HttpGet("qr")]
    public IActionResult GetQRCode(string codigo, string evento)
    {
        try
        {
            // Lógica para generar el código QR en base a los parámetros
            string qrCodeBase64 = GenerateQRCodeBase64(codigo, evento);
            return Ok(new { success = true, qrCodeBase64 });
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