using CC.ChatSupport.Application.Interfaces;
using CC.ChatSupport.Application.Models;
using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CC.ChatSupport.Application;

public class PollMonitorService : BackgroundService, IPollMonitorService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<PollHeartbeat> _channel;
    private readonly ConcurrentDictionary<int, int> _missedPolls = new();
    private readonly ConcurrentDictionary<int, DateTime> _lastPoll = new();

    public PollMonitorService
    (
        IServiceScopeFactory scopeFactory, 
        Channel<PollHeartbeat> channel
    )
    {
        _scopeFactory = scopeFactory;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            while (_channel.Reader.TryRead(out var heartbeat))
            {
                _lastPoll[heartbeat.SessionId] = heartbeat.Timestamp;
                _missedPolls[heartbeat.SessionId] = 0;
            }

            await CheckInactiveSessions();
            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task CheckInactiveSessions()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();

        var sessions = await db.ChatSessions
                                .Where(s => s.IsActive)
                                .ToListAsync();

        foreach (var session in sessions)
        {
            if (_lastPoll.TryGetValue(session.Id, out var last) 
                && DateTime.UtcNow.Subtract(last).TotalSeconds > 1.5)
            {
                _missedPolls.AddOrUpdate(session.Id, 1, (_, count) => count + 1);

                if (_missedPolls[session.Id] >= 3)
                {
                    session.IsActive = false;

                    var agent = await db.Agents.FindAsync(session.AssignedAgentId);
                    if (agent != null)
                    {
                        agent.ActiveChats--;
                    }
                }
            }
        }

        await db.SaveChangesAsync();
    }
}