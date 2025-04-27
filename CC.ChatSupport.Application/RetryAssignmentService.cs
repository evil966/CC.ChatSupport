using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CC.ChatSupport.Application;

public class RetryAssignmentService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RetryAssignmentService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();

            var now = DateTime.UtcNow;

            //-- Get all active, unassigned chat sessions
            var unassignedChats = await db.ChatSessions
                .Where(c => c.IsActive && c.AssignedAgentId == null)
                .ToListAsync(stoppingToken);

            if (unassignedChats.Any())
            {
                //-- Get available agents
                var agents = await db.Agents
                    .Include(a => a.Shift)
                    .Where(a => now.TimeOfDay >= a.Shift.Start && now.TimeOfDay <= a.Shift.End)
                    .OrderBy(a => a.Seniority)
                    .ToListAsync(stoppingToken);

                foreach (var chat in unassignedChats)
                {
                    foreach (var agent in agents)
                    {
                        if (agent.ActiveChats < agent.MaxConcurrency)
                        {
                            chat.AssignedAgentId = agent.Id;
                            agent.ActiveChats++;
                            break; //-- Move to next chat
                        }
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(5000, stoppingToken); 
        }
    }
}