namespace CC.ChatSupport.Domain;

public class ChatSession
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public int MissedPolls { get; set; } = 0;
    public int? AssignedAgentId { get; set; }
    public Agent? AssignedAgent { get; set; }
}
