namespace CC.ChatSupport.Domain;

public class Shift
{
    public int Id { get; set; }
    public TimeSpan Start { get; set; }
    public TimeSpan End { get; set; }

    public bool IsCurrentShift(DateTime time)
        => time.TimeOfDay >= Start && time.TimeOfDay <= End;
}
