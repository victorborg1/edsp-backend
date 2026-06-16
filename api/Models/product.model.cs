namespace api.Models
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string StripePriceId { get; set; }
        public required string FileName { get; set; }
        public string? Description { get; set; }
    }
}