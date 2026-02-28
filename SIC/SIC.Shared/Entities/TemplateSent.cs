namespace SIC.Shared.Entities
{
    public class TemplateSent
    {
        public int Id { get; set; }
        public int TemplateNumber { get; set; }
        public int InvitationId { get; set; }
        public Invitation? Invitation { get; set; } = null!;
    }
}