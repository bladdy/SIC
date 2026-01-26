using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SIC.Backend.Hubs;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.DTOs;
using SIC.Shared.Enums;
using System.Threading.Tasks;

[ApiController]
[Route("api/whatsapp/webhook")]
public class WhatsappWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IMessageUnitOfWork _iMessageUnitOfWork;
    private readonly IHubContext<WhatsappChatHub> _hub;

    public WhatsappWebhookController(IConfiguration configuration, IMessageUnitOfWork iMessageUnitOfWork, IHubContext<WhatsappChatHub> hub)
    {
        _hub = hub;
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

        var dto = new WhatsappIncomingMessageDto
        {
            MessageId = message.Id,
            From = message.From,
            Text = message.Text?.Body,
            Type = message.Type,
            ReplyToMessageId = message.Context?.Id,
            Direction = "IN",
            Status = ""
        };

        var response = await _iMessageUnitOfWork.AddReceiveMessages(dto);
        if (response.Success)
            return Ok();

        return BadRequest();
    }

    [HttpGet("chat/{phoneNumber}")]
    public async Task<IActionResult> GetChat(string phoneNumber)
    {
        var messages = await _iMessageUnitOfWork
            .GetConversationAsync(phoneNumber);

        return Ok(messages);
    }

    [HttpGet("whatsapp/inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var inbox = await _iMessageUnitOfWork.GetInboxAsync();
        return Ok(inbox);
    }

    [HttpPost("recive")]
    public async Task<IActionResult> Receivev2([FromBody] WhatsappWebhookPayload payload)
    {
        // 1️⃣ Extraemos los Value (messages + statuses viven aquí)
        var values = payload.Entry?
            .SelectMany(e => e.Changes)
            .Select(c => c.Value)
            .ToList();

        if (values == null || !values.Any())
            return Ok();

        // 2️⃣ Mensajes
        var messages = values
            .SelectMany(v => v.Messages ?? new List<MessageDTO>())
            .ToList();

        if (!messages.Any())
            return Ok();

        // 3️⃣ Estados
        var statuses = values
            .SelectMany(v => v.Statuses ?? new List<MessageStatus>())
            .ToList();

        // 4️⃣ Unimos mensaje + status por MessageId
        foreach (var message in messages)
        {
            var status = statuses.FirstOrDefault(s => s.Id == message.Id);

            var dto = new WhatsappIncomingMessageDto
            {
                MessageId = message.Id,
                From = message.From,
                Text = message.Text?.Body,
                Type = message.Type,
                ReplyToMessageId = message.Context?.Id,
                Timestamp = DateTime.UtcNow,
                Direction = "IN",
                Status = status?.Status
            };

            await _iMessageUnitOfWork.AddReceiveMessages(dto);
            await _hub.Clients.All.SendAsync("InboxUpdated");
            await _hub.Clients.Group(dto.From).SendAsync("NewMessage",
            new RealtimeChatMessageDto
            {
                PhoneNumber = dto.From,
                Direction = "IN",
                MessageType = "text",
                Content = dto.Text,
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok();
    }
}