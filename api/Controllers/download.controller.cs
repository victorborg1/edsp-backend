using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using api.Data;

namespace api.Controllers
{
    [ApiController]
    [Route("api/download")]
    public class DownloadController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DownloadController(MyDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [Authorize]
        [HttpGet("{productId}")]
        public async Task<IActionResult> Download(int productId)
        {
            // get user from jwt.
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            // does the user have a license?
            var hasLicense = await _context.Licenses
                .AnyAsync(l => l.UserId == userId && l.ProductId == productId);

            if (!hasLicense)
                return Unauthorized("You don't own this product");

            // get product.
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
                return NotFound("Product not found");

            // create secure file path, validate and return.
            var basePath = Path.Combine(_env.ContentRootPath, "protected", "plugins");

            var safeFileName = Path.GetFileName(product.FileName);
            var filePath = Path.Combine(basePath, safeFileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            return PhysicalFile(filePath, "application/zip", safeFileName);
        }
    }
}