using health_monitor.BackgroundServices;
using health_monitor.Components;
using health_monitor.Hub;
using health_monitor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

await builder.Services.ConfigureHealthCheckService(builder.Environment, "healthcheckconfig.json");
builder.Services.AddCors();
builder.Services.AddSignalR();
builder.Services.AddSingleton<StatusService>();
builder.Services.AddHostedService<HealthCheckServiceOrchestrator>();
builder.Services.AddHttpClient();

var app = builder.Build();

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
app.MapGet("/data", (StatusService statusService) => Results.Ok(statusService.GetServices()));
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(health_monitor.Client._Imports).Assembly);

app.Run();