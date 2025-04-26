using CC.ChatSupport.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CC.ChatSupport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly SupportDbContext _db;

    public DashboardController(SupportDbContext db)
    {
        _db = db;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var activeSessions = await _db.ChatSessions.CountAsync(c => c.IsActive);
        var totalSessions = await _db.ChatSessions.CountAsync();

        var agents = await _db.Agents.Include(a => a.Shift).ToListAsync();

        var agentSummaries = agents.Select(a => new
        {
            a.Name,
            a.Seniority,
            a.ActiveChats,
            a.MaxConcurrency,
            ShiftStart = a.Shift.Start,
            ShiftEnd = a.Shift.End
        });

        return Ok(new
        {
            ActiveChatSessions = activeSessions,
            TotalChatSessions = totalSessions,
            Agents = agentSummaries
        });
    }
}
