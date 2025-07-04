namespace health_monitor.Models;

public record StatusInfo(Status Status, string StatusMsg, TimeOnly CheckTime, int ResponseTime);