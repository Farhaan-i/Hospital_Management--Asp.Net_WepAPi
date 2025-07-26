using HMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorLogController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ErrorLogController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/ErrorLog
        [HttpPost]
        public async Task<IActionResult> LogError([FromBody] FrontendErrorLog log)
        {
            log.TimeOccurred = DateTime.UtcNow;

            // Optional: capture authenticated user info if available
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    log.UserId = userId;
                    log.Username = User.Identity.Name;
                }
            }

            _context.FrontendErrorLogs.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Error logged successfully" });
        }

        // GET: api/ErrorLog (Admin or Staff access only)
        [HttpGet]
        [Authorize(Policy = "StaffOnly")] // Or [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetErrors()
        {
            var logs = await _context.FrontendErrorLogs
                .OrderByDescending(e => e.TimeOccurred)
                .Take(100)
                .ToListAsync();

            return Ok(logs);
        }
    }
}
