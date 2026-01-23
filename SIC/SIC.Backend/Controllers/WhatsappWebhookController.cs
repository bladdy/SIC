using Microsoft.AspNetCore.Mvc;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using System.Threading.Tasks;

[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsappWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMessageUnitOfWork _iMessageUnitOfWork;

    public WhatsappWebhookController(IConfiguration configuration, IMessageUnitOfWork iMessageUnitOfWork)
    {
        _iMessageUnitOfWork = iMessageUnitOfWork;
        _configuration = configuration;
    }

    // 🔐 Verificación inicial de Meta
    [HttpGet]
    public IActionResult VerifyWebhook(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string token,
        [FromQuery(Name = "hub.challenge")] string challenge)
    {
        var accessToken = _configuration["WhatsApp:invboxv_webhook_token"];//Para que venga del coonfig

        if (mode == "subscribe" && token == accessToken)
        {
            return Ok(challenge);
        }

        return Unauthorized();
    }

    // 📩 Recepción de mensajes
    [HttpPost]
    public async Task<IActionResult> Receive([FromBody] WhatsappWebhookPayload payload)
    {
        var message = payload.Entry?
            .FirstOrDefault()?
            .Changes?
            .FirstOrDefault()?
            .Value?
            .Messages?
            .FirstOrDefault();

        if (message == null)
            return Ok();

        string from = message.From;
        string text = message.Text?.Body;
        string replyToMessageId = message.Context?.Id;

        var response = await _iMessageUnitOfWork.AddReceiveMessages(from, text, replyToMessageId);
        if (response.Success)
            return Ok();

        return BadRequest();
    }
}