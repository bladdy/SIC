namespace SIC.Shared.Entities
{
    public class PhotoEvent
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = null!;
        public string PortadaUrl { get; set; } = null!;

        public ICollection<PhotoEventImage> Images { get; set; } = new List<PhotoEventImage>();
    }
}