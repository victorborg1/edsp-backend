using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using api.Data;
using api.Models;

namespace api.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly MyDbContext _context;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(
            IConfiguration config,
            MyDbContext context,
            ILogger<WebhookController> logger)
        {
            _config = config;
            _context = context;
            _logger = logger;
        }

        // handle stripe events.
        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _config["Stripe:WebhookSecret"]
                );

                _logger.LogInformation("Stripe event: {EventType}", stripeEvent.Type);

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    return await HandleCheckoutCompleted(stripeEvent);
                }

                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Webhook error");
                return BadRequest();
            }
        }

        private async Task<IActionResult> HandleCheckoutCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            if (session == null)
                return BadRequest("Invalid session");

            if (session.Metadata == null ||
                !session.Metadata.ContainsKey("userId") ||
                !session.Metadata.ContainsKey("productId"))
            {
                return BadRequest("Missing metadata");
            }

            var userId = int.Parse(session.Metadata["userId"]);
            var productId = int.Parse(session.Metadata["productId"]);

            // update order as paid.
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.StripeSessionId == session.Id);

            if (order != null)
            {
                order.IsPaid = true;
            }

            // create license if not exists.
            var existingLicense = await _context.Licenses
                .FirstOrDefaultAsync(l =>
                    l.UserId == userId &&
                    l.ProductId == productId);

            if (existingLicense == null)
            {
                var license = new License
                {
                    UserId = userId,
                    ProductId = productId,
                    LicenseKey = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Licenses.Add(license);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}