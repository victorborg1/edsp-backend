using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using api.Data;
using api.Models;
using api.Dtos;

namespace api.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        private readonly MyDbContext _context;

        public PaymentController(MyDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create-checkout")]
        public async Task<ActionResult> CreateCheckout([FromBody] CreateCheckoutRequest request)
        {
            // get user from jwt.
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            // get product. 
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);

            if (product == null)
                return NotFound("Product not found");

            // does the user already own this product?
            var alreadyOwned = await _context.Licenses
                .AnyAsync(l => l.UserId == userId && l.ProductId == request.ProductId);

            if (alreadyOwned)
                return BadRequest("You already own this product");

            // create stripe checkout session.
            var domain = "https://edsp-client.vercel.app/"; //http://localhost:5173

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = product.StripePriceId,
                        Quantity = 1,
                    },
                },
                SuccessUrl = $"{domain}/success",
                CancelUrl = $"{domain}/cancel",

                Metadata = new Dictionary<string, string>
                {
                    { "userId", userId.ToString() },
                    { "productId", request.ProductId.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // create order.
            var order = new Order
            {
                UserId = userId,
                StripeSessionId = session.Id,
                IsPaid = false
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // return stripe url.
            return Ok(new { url = session.Url });
        }
    }
}