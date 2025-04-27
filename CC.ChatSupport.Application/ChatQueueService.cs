using CC.ChatSupport.Application.Interfaces;
using CC.ChatSupport.Domain;
using System.Threading.Channels;

namespace CC.ChatSupport.Application;

public class ChatQueueService : IChatQueueService
{
    private readonly Channel<ChatSession> _chatSessionChannel;

    public ChatQueueService(Channel<ChatSession> chatSessionChannel)
    {
        _chatSessionChannel = chatSessionChannel;
    }

    public async Task<ChatSession> EnqueueChatAsync()
    {
        var session = new ChatSession();
        await _chatSessionChannel.Writer.WriteAsync(session);
        return session;
    }
}