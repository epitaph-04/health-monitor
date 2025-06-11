using health_monitor.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using health_monitor.Client.Model;

namespace health_monitor.Services;

public static class ConfigurationService
{

    public static async Task ConfigureHealthCheckService(this IServiceCollection services, IWebHostEnvironment env, string configFileName)
    {
        try
        {
            var configFilePath = Path.Combine(env.ContentRootPath, configFileName);
            if (File.Exists(configFilePath))
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    Converters = { new JsonStringEnumConverter() }
                };
                await using var stream = File.OpenRead(configFilePath);
                await foreach (var application in JsonSerializer.DeserializeAsyncEnumerable<ApplicationConfiguration>(stream, jsonOptions))
                {
                    Console.WriteLine($"App name (type): {application!.Name} ({application!.Type}).");
                    services.AddSingleton<IHealthCheckService>(provider =>
                    {
                        return application.Type switch
                        {
                            ServiceType.Http => new HttpHealthCheckService(
                                provider.GetRequiredService<HttpClient>(), application, provider.GetRequiredService<ILogger<HttpHealthCheckService>>()),
                            ServiceType.Db => new DbHealthCheckService(application),
                            ServiceType.Sns => new SnsHealthCheckService(application),
                            ServiceType.Sqs => new SqsHealthCheckService(application),
                            ServiceType.Rabbitmq => new RabbitmqHealthCheckService(application),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    });
                }
            }
            else
            {
                Console.WriteLine($"Error: Configuration file '{configFilePath}' not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading or parsing configuration: {ex.Message}");
        }
    }
}