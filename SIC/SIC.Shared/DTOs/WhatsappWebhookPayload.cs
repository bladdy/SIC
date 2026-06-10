using System.Text.Json.Serialization;

namespace SIC.Shared.DTOs
{
    public class WhatsappWebhookPayload
    {
        [JsonPropertyName("entry")]
        public List<Entry> Entry { get; set; }
    }

    public class Entry
    {
        [JsonPropertyName("changes")]
        public List<Change> Changes { get; set; }
    }

    public class Change
    {
        [JsonPropertyName("value")]
        public Value Value { get; set; }
    }

    public class Value
    {
        [JsonPropertyName("messages")]
        public List<MessageDTO>? Messages { get; set; }

        [JsonPropertyName("statuses")]
        public List<MessageStatus>? Statuses { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }

    // =========================
    // MENSAJES
    // =========================
    public class MessageDTO
    {
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public TextContent? Text { get; set; }

        [JsonPropertyName("image")]
        public Media? Image { get; set; }

        [JsonPropertyName("context")]
        public MessageContext? Context { get; set; }
    }

    public class TextContent
    {
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }

    public class Media
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; }
    }

    public class MessageContext
    {
        [JsonPropertyName("from")]
        public string From { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }
    }

    // =========================
    // ESTADOS (DELIVERED, READ)
    // =========================
    public class MessageStatus
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("recipient_id")]
        public string RecipientId { get; set; }

        // NUEVO
        [JsonPropertyName("errors")]
        public List<StatusError>? Errors { get; set; }

        // OPCIONAL
        [JsonPropertyName("conversation")]
        public ConversationInfo? Conversation { get; set; }

        // OPCIONAL
        [JsonPropertyName("pricing")]
        public PricingInfo? Pricing { get; set; }
    }

    // =========================
    // METADATA
    // =========================
    public class Metadata
    {
        [JsonPropertyName("phone_number_id")]
        public string PhoneNumberId { get; set; }

        [JsonPropertyName("display_phone_number")]
        public string DisplayPhoneNumber { get; set; }
    }

    public class StatusError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("error_data")]
        public ErrorData? ErrorData { get; set; }
    }

    public class ErrorData
    {
        [JsonPropertyName("details")]
        public string? Details { get; set; }
    }

    public class ConversationInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("expiration_timestamp")]
        public string? ExpirationTimestamp { get; set; }

        [JsonPropertyName("origin")]
        public ConversationOrigin? Origin { get; set; }
    }

    public class ConversationOrigin
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class PricingInfo
    {
        [JsonPropertyName("billable")]
        public bool Billable { get; set; }

        [JsonPropertyName("pricing_model")]
        public string? PricingModel { get; set; }

        [JsonPropertyName("category")]
        public string? Category { get; set; }
    }
}