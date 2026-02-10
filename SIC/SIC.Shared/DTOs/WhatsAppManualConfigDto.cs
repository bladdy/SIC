using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.DTOs;

public class WhatsAppManualConfigDto
{
    public string SystemUserId { get; set; } = string.Empty;

    public string BusinessId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El WABA ID es obligatorio.")]
    public string WabaId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Phone Number ID es obligatorio.")]
    public string PhoneNumberId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El número de WhatsApp es obligatorio.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Access Token es obligatorio.")]
    public string AccessToken { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string WebhookUrl { get; set; } = string.Empty;

    public string WebhookVerificationToken { get; set; } = string.Empty;
}