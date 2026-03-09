using Microsoft.AspNetCore.Mvc;
using SysGuard.Monitor.API.Data;
using SysGuard.Monitor.Models;

namespace SysGuard.Monitor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MetricsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SystemStats stats)
        {
            if (stats == null) return BadRequest();

            _context.Metrics.Add(stats);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[DB] Data saved for {stats.MachineName}: CPU {stats.CpuUsage}%");
    
            return Ok(new { message = "Data saved successfully to database." });
        }
    }
}