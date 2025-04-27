using CC.ChatSupport.Application.Helpers;
using CC.ChatSupport.Application.Interfaces;
using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CC.ChatSupport.Application;

public class AgentChatCoordinatorService : BackgroundService, IAgentChatCoordinatorService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AgentChatCoordinatorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var unassignedChats 
                = await db.ChatSessions
                            .Where(c => c.IsActive && c.AssignedAgentId == null)
                            .ToListAsync(stoppingToken);

            if (unassignedChats.Any())
            {
                foreach (var chat in unassignedChats)
                {
                    _ = AssignAgentToChatAsync(chat, db);
                }
            }

            //--- Try to perform agent assignment every 3 seconds
            await Task.Delay(3000, stoppingToken); 
        }
    }

    public async Task<ChatSession> AssignAgentToChatAsync(ChatSession session, SupportDbContext db)
    {
        var now = DateTime.UtcNow;
        var activeSessions = await db.ChatSessions.CountAsync(s => s.IsActive);

        var nowTime = now.TimeOfDay;
        var teamAgents 
            = await db.Agents
                        .Include(a => a.Shift)
                        .Where(a => nowTime >= a.Shift.Start && nowTime <= a.Shift.End)
                        .ToListAsync();

        var normalCapacity = teamAgents.Sum(a => a.MaxConcurrency);
        var maxQueueLength = (int)(normalCapacity * 1.5);

        if (activeSessions >= maxQueueLength)
        {
            if (now.IsWithinOfficeHours())
            {
                var overflowAgents 
                    = await db.Agents
                                .Where(a => a.IsAuxiliary)
                                .ToListAsync();

                if (overflowAgents.All(a => a.ActiveChats >= a.MaxConcurrency))
                {
                    session.IsActive = false;
                    await db.SaveChangesAsync();
                    return session;
                }

                AssignAgentRoundRobin(overflowAgents, session);
                await db.SaveChangesAsync();
                return session;
            }

            session.IsActive = false;
            await db.SaveChangesAsync();
            return session;
        }

        AssignAgentRoundRobin(teamAgents, session);
        await db.SaveChangesAsync();
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
}
