namespace health_monitor.Models;

public class ApplicationConfiguration
{
    public string Name { get; set; } = null!;
    public ApplicationType Type { get; set; }
    public string Target { get; set; } = null!;
    public int ExpectedResponseCode { get; set; } = 200;
    public string Method { get; set; } = "GET";
    public string? RequestBody { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? Query { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}
