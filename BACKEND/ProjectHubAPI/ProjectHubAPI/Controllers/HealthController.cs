using Microsoft.AspNetCore.Mvc;
using ProjectHubAPI.Data;
using System;
using System.Threading.Tasks;

namespace ProjectHubAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                return Ok(new { Status = "Healthy", CanConnect = canConnect });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Status = "Unhealthy", Error = ex.Message });
            }
        }
    }
}
