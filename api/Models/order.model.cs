

namespace api.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string StripeSessionId { get; set; }
        public bool IsPaid { get; set; } = false;
    }
}