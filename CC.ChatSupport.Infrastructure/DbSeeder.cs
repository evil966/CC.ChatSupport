using CC.ChatSupport.Domain;

namespace CC.ChatSupport.Infrastructure;

public static class DbSeeder
{
    public static async Task SeedAsync(SupportDbContext db)
    {
        if (db.Agents.Any()) return;

        var shifts = new List<Shift>
        {
            new() { Start = TimeSpan.FromHours(0), End = TimeSpan.FromHours(8) },
            new() { Start = TimeSpan.FromHours(8), End = TimeSpan.FromHours(16) },
            new() { Start = TimeSpan.FromHours(16), End = TimeSpan.FromHours(23.99) }
        };

        db.Shifts.AddRange(shifts);
        await db.SaveChangesAsync();

        var shift1 = shifts[0];
        var shift2 = shifts[1];
        var shift3 = shifts[2];

        db.Agents.AddRange(new[]
        {
            // Team A
            new Agent { Name = "TeamA_Lead", Seniority = Seniority.TeamLead, Shift = shift1 },
            new Agent { Name = "TeamA_Mid1", Seniority = Seniority.Mid, Shift = shift1 },
            new Agent { Name = "TeamA_Mid2", Seniority = Seniority.Mid, Shift = shift1 },
            new Agent { Name = "TeamA_Junior", Seniority = Seniority.Junior, Shift = shift1 },

            // Team B
            new Agent { Name = "TeamB_Senior", Seniority = Seniority.Senior, Shift = shift2 },
            new Agent { Name = "TeamB_Mid", Seniority = Seniority.Mid, Shift = shift2 },
            new Agent { Name = "TeamB_Junior1", Seniority = Seniority.Junior, Shift = shift2 },
            new Agent { Name = "TeamB_Junior2", Seniority = Seniority.Junior, Shift = shift2 },

            // Team C (Night)
            new Agent { Name = "TeamC_Mid1", Seniority = Seniority.Mid, Shift = shift3 },
            new Agent { Name = "TeamC_Mid2", Seniority = Seniority.Mid, Shift = shift3 }
        });

        // Overflow (Junior-like)
        db.Agents
            .AddRange(Enumerable.Range(1, 6)
            .Select(i => new Agent 
                        { 
                            Name = $"Overflow{i}", 
                            Seniority = Seniority.Junior, 
                            Shift = shift2 
                        }
            ));

        // Optional: add test sessions
        for (int i = 0; i < 5; i++)
        {
            db.ChatSessions.Add(new ChatSession
            {
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                IsActive = true
            });
        }

        await db.SaveChangesAsync();
    }
}