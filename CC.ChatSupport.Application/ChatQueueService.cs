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
        var now = DateTime.UtcNow;

        var activeSessions = await _db.ChatSessions.CountAsync(s => s.IsActive);

        var nowTime = now.TimeOfDay;
        var teamAgents = await _db.Agents.Include(a => a.Shift)
                                         .Where(a => nowTime >= a.Shift.Start && nowTime <= a.Shift.End)
                                         .ToListAsync();

        var normalCapacity = teamAgents.Sum(a => a.MaxConcurrency);
        var maxQueueLength = (int)(normalCapacity * 1.5);

        if (activeSessions >= maxQueueLength)
        {
            if (IsWithinOfficeHours(now))
            {
                var overflowAgents = await 
                    _db.Agents
                        .Where(a => a.Name.StartsWith("Overflow"))
                        .ToListAsync();

                if (overflowAgents.All(a => a.ActiveChats >= a.MaxConcurrency))
                {
                    session.IsActive = false;
                    _db.ChatSessions.Add(session);
                    await _db.SaveChangesAsync();
                    return session;
                }

                AssignAgentRoundRobin(overflowAgents, session);
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();
                return session;
            }

            session.IsActive = false;
            _db.ChatSessions.Add(session);
            await _db.SaveChangesAsync();
            return session;
        }

        AssignAgentRoundRobin(teamAgents, session);
        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    private void AssignAgentRoundRobin(List<Agent> agents, ChatSession session)
    {
        foreach (var agent in agents.OrderBy(a => a.Seniority))
        {
            if (agent.ActiveChats < agent.MaxConcurrency)
            {
                agent.ActiveChats++;
                session.AssignedAgentId = agent.Id;
                break;
            }
        }
    }

    private bool IsWithinOfficeHours(DateTime now)
    {
        var t = now.TimeOfDay;
        return t >= TimeSpan.FromHours(8) && t <= TimeSpan.FromHours(17);
    }
}