using CC.ChatSupport.Domain;
using Microsoft.EntityFrameworkCore;

namespace CC.ChatSupport.Infrastructure;

public class SupportDbContext : DbContext
{
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Shift> Shifts => Set<Shift>();

    public SupportDbContext(DbContextOptions<SupportDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ChatSession>()
            .HasOne(c => c.AssignedAgent)
            .WithMany()
            .HasForeignKey(c => c.AssignedAgentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}