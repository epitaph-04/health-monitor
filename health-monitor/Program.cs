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
app.Run();