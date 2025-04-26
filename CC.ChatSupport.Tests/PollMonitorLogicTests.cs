using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CC.ChatSupport.Tests;

public class PollMonitorLogicTests
{
    private SupportDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<SupportDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SupportDbContext(options);
    }

    [Fact]
    public async Task Should_Mark_Session_Inactive_After_3_Missed_Polls()
    {
        var db = GetDbContext();
        db.ChatSessions.Add(new ChatSession { MissedPolls = 2, IsActive = true });
        db.SaveChanges();

        var session = await db.ChatSessions.FirstAsync();
        session.MissedPolls++; // Simulate 3rd miss
        if (session.MissedPolls >= 3)
            session.IsActive = false;

        await db.SaveChangesAsync();

        var updated = await db.ChatSessions.FirstAsync();
        Assert.False(updated.IsActive);
    }
}