using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using CC.ChatSupport.Application;
using CC.ChatSupport.Infrastructure;

namespace CC.ChatSupport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatQueueService _queueService;
    private readonly SupportDbContext _db;

    public ChatController(ChatQueueService queueService, SupportDbContext db)
    {
        _queueService = queueService;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat()
    {
        var session = await _queueService.EnqueueChatAsync();
        return Ok(new
        {
            session.Id,
            AssignedAgent = session.AssignedAgent?.Name ?? "Unassigned"
        });
    }

    [HttpGet("{id}/poll")]
    public async Task<IActionResult> PollChat(int id)
    {
        var session = await _db.ChatSessions.FindAsync(id);
        if (session is null || !session.IsActive)
            return NotFound("Session inactive or not found");

        session.MissedPolls = 0;
        await _db.SaveChangesAsync();

        return Ok("OK");
    }
}