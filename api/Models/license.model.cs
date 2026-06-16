
namespace api.Models
{
    public class License
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public required string LicenseKey { get; set; }
        public DateTime CreatedAt { get; set; }
        public User? User { get; set; }
    }
}