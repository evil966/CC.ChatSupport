using CC.ChatSupport.Domain;

namespace CC.ChatSupport.Application;

public class AgentChatCoordinatorService
{
    public void AssignAgentRoundRobin(List<Agent> agents, ChatSession session)
    {
        foreach (var agent in agents.OrderBy(a => a.Seniority))
        {
            if (agent.ActiveChats < agent.MaxConcurrency)
            {
                agent.ActiveChats++;
                session.AssignedAgentId = agent.Id;
                break;
            }
        }
    }
}