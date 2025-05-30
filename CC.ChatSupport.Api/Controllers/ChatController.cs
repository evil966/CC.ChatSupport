﻿using CC.ChatSupport.Application.Interfaces;
using CC.ChatSupport.Application.Models;
using CC.ChatSupport.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

namespace CC.ChatSupport.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatQueueService _queueService;
    private readonly SupportDbContext _db;
    private readonly Channel<PollHeartbeat> _channel;

    public ChatController
    (
        IChatQueueService queueService, 
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
            Status = "Chat Session created. Waiting for agent assignment..."
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetChatStatus(int id)
    {
        var session = await _db.ChatSessions
                                .Include(s => s.AssignedAgent)
                                .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
        {
            return NotFound("Chat session not found");
        }

        return Ok(new
        {
            session.Id,
            session.IsActive,
            AssignedAgent = session.AssignedAgent != null ? session.AssignedAgent.Name : null,
            Status = session.AssignedAgent != null
                ? "Agent assigned"
                : "Waiting for agent assignment..."
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