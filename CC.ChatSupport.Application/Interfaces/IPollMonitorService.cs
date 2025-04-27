namespace CC.ChatSupport.Application.Interfaces;

public interface IPollMonitorService
{
    Task CheckInactiveSessions();
}
