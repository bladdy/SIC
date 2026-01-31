using Microsoft.AspNetCore.Mvc;

namespace SIC.Backend.Controllers;

[ApiController]
[Route("api/whatsapp")]
public class WhatsAppAuthController : ControllerBase
{
    [HttpPost("exchange-code")]
    public IActionResult ExchangeCode([FromBody] ExchangeCodeRequest request)
    {
        // Aquí:
        // 1. Intercambias code → access_token (Meta OAuth)
        // 2. Guardas business_id, waba_id
        // 3. Token largo (60 días)
        return Ok();
    }
}

public record ExchangeCodeRequest(string Code);