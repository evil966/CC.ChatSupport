using CC.ChatSupport.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CC.ChatSupport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly SupportDbContext _db;

    public HealthController(SupportDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetHealth()
    {
        try
        {
            var agentCount = await _db.Agents.CountAsync();
            var sessionCount = await _db.ChatSessions.CountAsync();

            return Ok(new
            {
                Status = "Healthy",
                Agents = agentCount,
                Sessions = sessionCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}