using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace CC.ChatSupport.Application;

public class PollMonitorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PollMonitorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();

            var sessions = await db.ChatSessions
                .Include(s => s.AssignedAgent)
                .Where(s => s.IsActive)
                .ToListAsync(stoppingToken);

            foreach (var session in sessions)
            {
                session.MissedPolls++;

                if (session.MissedPolls >= 3)
                {
                    session.IsActive = false;

                    if (session.AssignedAgent != null)
                        session.AssignedAgent.ActiveChats--;
                }
            }

            await db.SaveChangesAsync(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}