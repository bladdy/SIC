using System.ComponentModel.DataAnnotations;

namespace SIC.Shared.Entities
{
    public class InvitationSendLog
    {
        public int Id { get; set; }

        [Required]
        public int InvitationId { get; set; }

        public Invitation? Invitation { get; set; }

        [Required]
        public DateTime SendDate { get; set; } = DateTime.Now;

        [Required]
        public bool IsSuccessful { get; set; }

        [MaxLength(250)]
        public string? WhatsAppMessageId { get; set; }

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        public int AttemptNumber { get; set; } = 1;
    }
}