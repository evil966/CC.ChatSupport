using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;


namespace CC.ChatSupport.Application;

public class ChatQueueService
{
    private readonly SupportDbContext _db;

    public ChatQueueService(SupportDbContext db)
    {
        _db = db;
    }

    public async Task<ChatSession> EnqueueChatAsync()
    {
        var session = new ChatSession();
        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync();

        await AssignAgentAsync(session);
        return session;
    }

    private async Task AssignAgentAsync(ChatSession session)
    {
        var now = DateTime.UtcNow;
        var availableAgents = await _db.Agents
            .Include(a => a.Shift)
            .Where(a => a.Shift.IsCurrentShift(now))
            .OrderBy(a => a.Seniority)
            .ToListAsync();

        foreach (var agent in availableAgents)
        {
            if (agent.ActiveChats < agent.MaxConcurrency)
            {
                agent.ActiveChats++;
                session.AssignedAgentId = agent.Id;
                await _db.SaveChangesAsync();
                break;
            }
        }
    }
}