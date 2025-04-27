using CC.ChatSupport.Application;
using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CC.ChatSupport.Tests;

public class ChatQueueServiceTests
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
    public async Task Should_Enqueue_Chat_And_Assign_Agent()
    {
        var db = GetDbContext();
        db.Agents
            .Add(CreateAgent(
                name: "Agent1", 
                level: Seniority.Junior, 
                start: TimeSpan.Zero, 
                end: TimeSpan.FromHours(23)
            ));
        db.SaveChanges();

        var coordinator = new AgentChatCoordinatorService();
        var service = new ChatQueueService(db, coordinator);
        var session = await service.EnqueueChatAsync();

        Assert.NotNull(session);
        Assert.NotNull(session.AssignedAgentId);
    }

    [Fact]
    public async Task Should_Not_Assign_When_Agents_At_Max_Capacity()
    {
        var db = GetDbContext();
        db.Agents
            .Add(CreateAgent(
                name: "BusyAgent", 
                level: Seniority.Junior, 
                start: TimeSpan.Zero, 
                end: TimeSpan.FromHours(23),
                activeChats: 4
            ));
        db.SaveChanges();

        var coordinator = new AgentChatCoordinatorService();
        var service = new ChatQueueService(db, coordinator);
        var session = await service.EnqueueChatAsync();

        Assert.Null(session.AssignedAgentId);
    }

    [Fact]
    public async Task Should_Assign_Junior_Before_Senior()
    {
        var db = GetDbContext();
        db.Agents.Add(CreateAgent("SeniorAgent", Seniority.Senior, TimeSpan.Zero, TimeSpan.FromHours(23)));
        db.Agents.Add(CreateAgent("JuniorAgent", Seniority.Junior, TimeSpan.Zero, TimeSpan.FromHours(23)));
        db.SaveChanges();

        var coordinator = new AgentChatCoordinatorService();
        var service = new ChatQueueService(db, coordinator);
        var session = await service.EnqueueChatAsync();

        var agent = await db.Agents.FindAsync(session.AssignedAgentId);
        Assert.Equal("JuniorAgent", agent.Name);
    }
}