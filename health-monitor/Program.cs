using health_monitor.BackgroundServices;
using health_monitor.Components;
using health_monitor.Hub;
using health_monitor.Services;
using Microsoft.AspNetCore.HttpOverrides;

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
builder.Services.AddSingleton<health_monitor.Services.Alerting.IAlertingService, health_monitor.Services.Alerting.AlertingService>();
builder.Services.AddSingleton<health_monitor.Services.Dependencies.IDependencyService, health_monitor.Services.Dependencies.DependencyService>();

// Phase 2 Services
builder.Services.AddSingleton<health_monitor.Services.Metrics.IMetricsService, health_monitor.Services.Metrics.MetricsService>();
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
    .AddAdditionalAssemblies(typeof(health_monitor.Client._Imports).Assembly);
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();