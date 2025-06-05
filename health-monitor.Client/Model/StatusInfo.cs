namespace health_monitor.Client.Model;

public record StatusInfo(Status Status, string StatusMsg, TimeOnly CheckTime, int ResponseTime);