using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using Services.Interfaces;
using Shared;


var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", true, false)
    .AddEnvironmentVariables()
    .Build();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient("OpenWeatherMapApiHttpClient", client =>
        {
            client.BaseAddress = new Uri(configuration["OpenWeatherMapApiConfig:BaseUrl"]!);
        });
        services.AddScoped<IWeatherService, OpenWeatherMapService>();
        services.AddSingleton<IWeatherRepository, CosmosDbWeatherRepository>();
        services.Configure<OpenWeatherMapApiConfig>(configuration.GetSection("OpenWeatherMapApiConfig"));
        services.Configure<CosmosDbConfig>(configuration.GetSection("CosmosDbConfig"));
    })
.Build();

using (var serviceScope = host.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var weatherRepository = services.GetRequiredService<IWeatherRepository>();

    // prepare db
    weatherRepository.Initialize();

}

host.Run();
