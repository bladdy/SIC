namespace SIC.Shared.Entities
{
    public class StripeEventLog
    {
        public int Id { get; set; }

        public string EventId { get; set; } = string.Empty;

        public DateTime ProcessedAt { get; set; }
    }
}
