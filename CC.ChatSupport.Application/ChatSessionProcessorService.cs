using CC.ChatSupport.Application.Interfaces;
using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Channels;

namespace CC.ChatSupport.Application;

public class ChatSessionProcessorService : BackgroundService, IChatSessionProcessorService
{
    private readonly Channel<ChatSession> _chatSessionChannel;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChatSessionProcessorService
    (
        Channel<ChatSession> chatSessionChannel,
        IServiceScopeFactory scopeFactory
    )
    {
        _chatSessionChannel = chatSessionChannel;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var session = await _chatSessionChannel.Reader.ReadAsync(stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SupportDbContext>();

            db.ChatSessions.Add(session);
            await db.SaveChangesAsync();
        }
    }
}
