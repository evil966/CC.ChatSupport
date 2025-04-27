using CC.ChatSupport.Domain;

namespace CC.ChatSupport.Application.Interfaces;

public interface IChatQueueService
{
    Task<ChatSession> EnqueueChatAsync();
}
