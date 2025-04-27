namespace CC.ChatSupport.Application.Helpers;

public static class TimeExtensions
{
    public static bool IsWithinOfficeHours(this DateTime timeUtc)
    {
        var localTime = timeUtc.TimeOfDay;
        return localTime >= TimeSpan.FromHours(8) && localTime <= TimeSpan.FromHours(17);
    }
}