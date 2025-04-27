using CC.ChatSupport.Application;
using CC.ChatSupport.Application.Models;
using CC.ChatSupport.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace CC.ChatSupport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatQueueService _queueService;
    private readonly SupportDbContext _db;
    private readonly Channel<PollHeartbeat> _channel;

    public ChatController
    (
        ChatQueueService queueService, 
        SupportDbContext db, 
        Channel<PollHeartbeat> channel
    )
    {
        _queueService = queueService;
        _db = db;
        _channel = channel;
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat()
    {
        var session = await _queueService.EnqueueChatAsync();
        return Ok(new
        {
            session.Id,
            AssignedAgent 
                = session.AssignedAgentId.HasValue 
                ? $"Agent #{session.AssignedAgentId}: {session?.AssignedAgent?.Name}" 
                : "Unassigned"
        });
    }

    [HttpGet("{id}/poll")]
    public async Task<IActionResult> PollChat(int id)
    {
        var session = await _db.ChatSessions.FindAsync(id);
        if (session is null || !session.IsActive)
        {
            return NotFound("Session inactive or not found");
        }

        await _channel.Writer.WriteAsync(new PollHeartbeat
        {
            SessionId = id,
            Timestamp = DateTime.UtcNow
        });

        return Ok(session);
    }
}