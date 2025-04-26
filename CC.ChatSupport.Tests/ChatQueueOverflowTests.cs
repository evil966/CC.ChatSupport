using CC.ChatSupport.Application;
using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CC.ChatSupport.Tests;

public class ChatQueueOverflowTests
{
    private SupportDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<SupportDbContext>()
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options;
        return new SupportDbContext(options);
    }

    private Agent CreateAgent
    (
        string name, 
        Seniority level, 
        TimeSpan start, 
        TimeSpan end, 
        int activeChats = 0
    )
    {
        return new Agent
        {
            Name = name,
            Seniority = level,
            ActiveChats = activeChats,
            Shift = new Shift { Start = start, End = end }
        };
    }

    [Fact]
    public async Task Should_Assign_Overflow_Agent_When_Team_Full()
    {
        var db = GetDbContext();

        for (int i = 0; i < 6; i++)
        {
            db.Agents
                .Add(CreateAgent(
                    name: $"Overflow{i}", 
                    level: Seniority.Junior, 
                    start: TimeSpan.FromHours(8), 
                    end: TimeSpan.FromHours(17)
                ));
        }
        db.SaveChanges();

        var service = new ChatQueueService(db);
        var session = await service.EnqueueChatAsync();

        Assert.NotNull(session.AssignedAgentId);
    }

    [Fact]
    public async Task Should_Reject_When_Overflow_Agents_Also_Busy()
    {
        var db = GetDbContext();

        for (int i = 0; i < 6; i++)
        {
            db.Agents
                .Add(CreateAgent(
                    name: $"Overflow{i}", 
                    level: Seniority.Junior, 
                    start: TimeSpan.FromHours(8), 
                    end: TimeSpan.FromHours(17), 
                    activeChats: 4
                ));
        }

        db.SaveChanges();

        var service = new ChatQueueService(db);
        var session = await service.EnqueueChatAsync();

        Assert.Null(session.AssignedAgentId);
    }
}
