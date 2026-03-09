using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SysGuard.Monitor.API.Data;
using SysGuard.Monitor.Models;

namespace SysGuard.Monitor.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController(AppDbContext context) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SystemStats? stats)
        {
            if (stats == null) return BadRequest();

            context.Metrics.Add(stats);
            await context.SaveChangesAsync();

            Console.WriteLine($"[DB] Data saved for {stats.MachineName}: CPU {stats.CpuUsage}%");
    
            return Ok(new { message = "Data saved successfully to database." });
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await context.Metrics
                .OrderByDescending(x => x.CapturedAt)
                .Take(20)
                .ToListAsync();

            return Ok(data);
        }
    }
}