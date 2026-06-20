
namespace SIC.Shared.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string URLImagen { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public decimal PriceTotal { get; set; }

        public List<string> Items { get; set; } = new();
    }
}
