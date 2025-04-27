using CC.ChatSupport.Domain;
using CC.ChatSupport.Infrastructure;

namespace CC.ChatSupport.Application.Interfaces;
public interface IAgentChatCoordinatorService
{
    Task<ChatSession> AssignAgentToChatAsync(ChatSession session, SupportDbContext db);
}
