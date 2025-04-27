using CC.ChatSupport.Application.Helpers;
using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CC.ChatSupport.Application;

public class ChatQueueService
{
    private readonly SupportDbContext _db;
    private readonly AgentChatCoordinatorService _coordinator;

    public ChatQueueService(SupportDbContext db, AgentChatCoordinatorService coordinator)
    {
        _db = db;
        _coordinator = coordinator;
    }

    public async Task<ChatSession> EnqueueChatAsync()
    {
        var session = new ChatSession();
        var now = DateTime.UtcNow;

        var activeSessions = await _db.ChatSessions.CountAsync(s => s.IsActive);

        var nowTime = now.TimeOfDay;
        var teamAgents = await _db.Agents
                .Include(a => a.Shift)
                .Where(a => nowTime >= a.Shift.Start && nowTime <= a.Shift.End)
                .ToListAsync();

        var normalCapacity = teamAgents.Sum(a => a.MaxConcurrency);
        var maxQueueLength = (int)(normalCapacity * 1.5);

        if (activeSessions >= maxQueueLength)
        {
            if (now.IsWithinOfficeHours())
            {
                var overflowAgents = await 
                    _db.Agents
                        .Where(a => a.IsAuxiliary)
                        .ToListAsync();

                if (overflowAgents.All(a => a.ActiveChats >= a.MaxConcurrency))
                {
                    session.IsActive = false;
                    _db.ChatSessions.Add(session);
                    await _db.SaveChangesAsync();
                    return session;
                }

                _coordinator.AssignAgentRoundRobin(overflowAgents, session);
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();
                return session;
            }

            session.IsActive = false;
            _db.ChatSessions.Add(session);
            await _db.SaveChangesAsync();
            return session;
        }

        _coordinator.AssignAgentRoundRobin(teamAgents, session);
        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

}