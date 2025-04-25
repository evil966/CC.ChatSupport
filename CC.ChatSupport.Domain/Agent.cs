namespace CC.ChatSupport.Domain;

public enum Seniority { Junior, Mid, Senior, TeamLead }

public class Agent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Seniority Seniority { get; set; }
    public int ActiveChats { get; set; }
    public Shift Shift { get; set; }
    public int ShiftId { get; set; }

    public double Efficiency => Seniority switch
    {
        Seniority.Junior => 0.4,
        Seniority.Mid => 0.6,
        Seniority.Senior => 0.8,
        Seniority.TeamLead => 0.5,
        _ => 0.0
    };

    public int MaxConcurrency => (int)(10 * Efficiency);
}