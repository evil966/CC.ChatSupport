using CC.ChatSupport.Application;
using CC.ChatSupport.Application.Models;
using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using ChatSupport.Application;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context
builder.Services.AddDbContext<SupportDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Channel for Poll Heartbeats
builder.Services.AddSingleton(Channel.CreateUnbounded<PollHeartbeat>());
builder.Services.AddSingleton(Channel.CreateUnbounded<ChatSession>());

// Services
builder.Services.AddScoped<ChatQueueService>();
builder.Services.AddHostedService<ChatSessionProcessorService>();
builder.Services.AddHostedService<AgentChatCoordinatorService>();
builder.Services.AddHostedService<PollMonitorService>();

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Middleware
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
