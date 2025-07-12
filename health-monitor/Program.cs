using health_monitor.BackgroundServices;
using health_monitor.Components;
using health_monitor.Hub;
using health_monitor.Models;
using health_monitor.Services;
using health_monitor.Services.Alerting;
using health_monitor.Services.Analytics;
using health_monitor.Services.Dependencies;
using health_monitor.Services.Metrics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

await builder.Services.ConfigureHealthCheckService(builder.Environment, "healthcheckconfig.json");
builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddSingleton<StatusService>();

// Phase 1 Services
builder.Services.AddSingleton<IAlertingService, AlertingService>();
builder.Services.AddSingleton<IDependencyService, DependencyService>();

// Phase 2 Services
builder.Services.AddSingleton<IMetricsService, MetricsService>();
builder.Services.AddSingleton<health_monitor.Services.Maintenance.IMaintenanceService, health_monitor.Services.Maintenance.MaintenanceService>();
builder.Services.AddControllers();

// Phase 3 Services - Advanced Intelligence & Automation
builder.Services.AddSingleton<health_monitor.Services.Intelligence.IPredictiveAnalysisService, health_monitor.Services.Intelligence.PredictiveAnalysisService>();
builder.Services.AddSingleton<health_monitor.Services.Recovery.IRecoveryService, health_monitor.Services.Recovery.RecoveryService>();

// Advanced Analytics Services
builder.Services.AddSingleton<health_monitor.Services.Analytics.IAdvancedAnalyticsService, health_monitor.Services.Analytics.AdvancedAnalyticsService>();

builder.Services.AddHostedService<HealthCheckServiceOrchestrator>();
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks();
builder.Services.Configure<ForwardedHeadersOptions>(opt =>
{
    opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost |
                           ForwardedHeaders.XForwardedProto;
    opt.KnownNetworks.Clear();
    opt.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.UseAntiforgery();
app.MapHub<NotificationHub>("notification");
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(health_monitor.Client._Imports).Assembly, typeof(HttpClient).Assembly);
app.MapHealthChecks("/health");
var healthCheckApi = app.MapGroup("/api/HealthCheckApi");
healthCheckApi.MapGet("/services", (StatusService statusService) => TypedResults.Ok(statusService.GetServices()));
healthCheckApi.MapGet("/services/{id}/status", (string id, StatusService svc) =>
{
    var services = svc.GetServices();
    return TypedResults.Ok(services.FirstOrDefault(s => s.Id == id));
});
healthCheckApi.MapGet("/services/{id}/metrics", async (IMetricsService metricsService, string id, [FromQuery] int days = 30) =>
{
    var period = TimeSpan.FromDays(days);
    var metrics = await metricsService.CalculateMetrics(id, period);
    return TypedResults.Ok(metrics);
});
healthCheckApi.MapGet("/services/{id}/sla", async (IMetricsService metricsService, string id, SlaReportingPeriod period = SlaReportingPeriod.Monthly) =>
{
    var metrics = await metricsService.GenerateSlaReport(id, period);
    return TypedResults.Ok(metrics);
});
healthCheckApi.MapGet("/services/{id}/dependencies", async (string id, IDependencyService dependencyService) =>
{
    var metrics = await dependencyService.GetDependenciesOf(id);
    return TypedResults.Ok(metrics);
});
healthCheckApi.MapGet("/services/{id}/dependents", async (string id, IDependencyService dependencyService) =>
{
    var metrics = await dependencyService.GetDependentsOf(id);
    return TypedResults.Ok(metrics);
});
healthCheckApi.MapGet("/services/{id}/check", async (string id, IEnumerable<IHealthCheckService> healthCheckServices) =>
{
    var service = healthCheckServices.FirstOrDefault(s => s.Id == id);
    var result = await service!.CheckHealthAsync();
    return TypedResults.Ok(result);
});
healthCheckApi.MapGet("/health/summary", (StatusService statusService) =>
{
    Status GetOverallStatus(Service[] services)
    {
        if (services.Any(s => s.LastCheckStatus.Status == Status.Critical))
            return Status.Critical;
        if (services.Any(s => s.LastCheckStatus.Status == Status.Degraded))
            return Status.Degraded;
        if (services.Any(s => s.LastCheckStatus.Status == Status.Unknown))
            return Status.Unknown;
        return Status.Healthy;
    }
    
    var services = statusService.GetServices();
    var summary = new
    {
        TotalServices = services.Length,
        HealthyServices = services.Count(s => s.LastCheckStatus.Status == Status.Healthy),
        DegradedServices = services.Count(s => s.LastCheckStatus.Status == Status.Degraded),
        CriticalServices = services.Count(s => s.LastCheckStatus.Status == Status.Critical),
        UnknownServices = services.Count(s => s.LastCheckStatus.Status == Status.Unknown),
        OverallStatus = GetOverallStatus(services),
        LastUpdated = DateTime.UtcNow
    };
    return TypedResults.Ok(summary);
});
healthCheckApi.MapGet("/alerts/rules", async (IAlertingService alertingService) =>
{
    var rules = await alertingService.GetAlertRules();
    return TypedResults.Ok(rules);
});
healthCheckApi.MapPost("/alerts/rules", async ([FromBody] AlertRule[] rules, IAlertingService alertingService) =>
{
    await alertingService.ConfigureAlertRules(rules);
    return TypedResults.Ok(new { message = "Alert rules configured successfully", count = rules.Length });
});
healthCheckApi.MapGet("/dependencies/graph", async (IDependencyService dependencyService) =>
{
    var graph = await dependencyService.BuildDependencyGraph();
    return TypedResults.Ok(graph);
});
healthCheckApi.MapGet("services/{id}/root-cause", async (string id, IDependencyService dependencyService) =>
{
    var rootCauses = await dependencyService.FindRootCause(id);
    return TypedResults.Ok(rootCauses);
});
var analyticsApi = app.MapGroup("/api/AnalyticsApi");
analyticsApi.MapGet("/services/{id}/timeseries", async (
    IAdvancedAnalyticsService analyticsService, 
    string id, 
    [FromQuery] TimeSeriesMetric metric = TimeSeriesMetric.ResponseTime, 
    [FromQuery] int days = 7) => 
{
    var analysis = await analyticsService.AnalyzeTimeSeries(id, TimeSpan.FromDays(days), metric);
    return TypedResults.Ok(analysis);
});
analyticsApi.MapPost("/correlations", async (IAdvancedAnalyticsService analyticsService, [FromBody] CorrelationRequest request) =>
{
    var analysis = await analyticsService.AnalyzeCorrelations(request.ServiceIds, TimeSpan.FromDays(request.Days));
    return TypedResults.Ok(analysis);
});
analyticsApi.MapGet("/services/{id}/capacity", async (
    IAdvancedAnalyticsService analyticsService, 
    string id,
    [FromQuery] int historicalDays = 30, 
    [FromQuery] int forecastDays = 30) => 
{
    var report = await analyticsService.GenerateCapacityPlan(
        id, 
        TimeSpan.FromDays(historicalDays), 
        TimeSpan.FromDays(forecastDays));
    return TypedResults.Ok(report);
});
analyticsApi.MapGet("/services/{id}/regression", async (
    IAdvancedAnalyticsService analyticsService, string id, [FromQuery] int days = 14) => 
{
    var report = await analyticsService.DetectPerformanceRegression(id, TimeSpan.FromDays(days));
    return TypedResults.Ok(report);
});
analyticsApi.MapGet("/services/{id}/benchmark", async (
    IAdvancedAnalyticsService analyticsService, string id, [FromBody] BenchmarkRequest request) => 
{
    var benchmark = await analyticsService.BenchmarkService(id, request.CompareServiceIds, TimeSpan.FromDays(request.Days));
    return TypedResults.Ok(benchmark);
});
analyticsApi.MapGet("/alerts/effectiveness", async (
    IAdvancedAnalyticsService analyticsService, [FromQuery] int days = 30) => 
{
    var report = await analyticsService.AnalyzeAlertEffectiveness(TimeSpan.FromDays(days));
    return TypedResults.Ok(report);
});
analyticsApi.MapGet("/insights", async (
    IAdvancedAnalyticsService analyticsService, [FromQuery] int days = 30) => 
{
    var insights = await analyticsService.GenerateSystemInsights(TimeSpan.FromDays(days));
    return TypedResults.Ok(insights);
});

analyticsApi.MapGet("/health-trend", async (
    IAdvancedAnalyticsService analyticsService, [FromQuery] int days = 7) => 
{
    var trend = await analyticsService.GenerateHealthTrend(TimeSpan.FromDays(days));
    return TypedResults.Ok(trend);
});
analyticsApi.MapPost("/custom", async (
    IAdvancedAnalyticsService analyticsService, [FromBody]CustomAnalyticsQuery query) => 
{
    var report = await analyticsService.RunCustomAnalysis(query);
    return TypedResults.Ok(report);
});
analyticsApi.MapPost("/export", async (
    IAdvancedAnalyticsService analyticsService, [FromBody] DataExportRequest request) => 
{
    var export = await analyticsService.ExportData(request);
            
    var contentType = request.Format switch
    {
        ExportFormat.JSON => "application/json",
        ExportFormat.CSV => "text/csv",
        ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ExportFormat.XML => "application/xml",
        _ => "application/octet-stream"
    };

    var fileName = $"health-check-data-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{request.Format.ToString().ToLower()}";
    return TypedResults.File(export.Data, contentType, fileName);
});
app.MapControllers();
app.Run();

class CorrelationRequest
{
    public string[] ServiceIds { get; set; } = [];
    public int Days { get; set; } = 7;
}

class BenchmarkRequest
{
    public string[] CompareServiceIds { get; set; } = [];
    public int Days { get; set; } = 30;
}