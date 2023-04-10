using Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace BackgroundJobs
{
    public class WeatherJob
    {
        private readonly ILogger _logger;
        private readonly IWeatherService _weatherService;
        private readonly IWeatherRepository _weatherRepository;

        public WeatherJob(ILoggerFactory loggerFactory, IWeatherService weatherService, IWeatherRepository weatherRepository)
        {
            _weatherService = weatherService;
            _weatherRepository = weatherRepository;
            _logger = loggerFactory.CreateLogger<WeatherJob>();
        }

        [Function("WeatherJob")]
        public async Task Run([TimerTrigger("0 */1 * * * *", RunOnStartup = true)] MyInfo myTimer)
        {
            _logger.LogInformation($"WeatherJob started at: {DateTime.UtcNow}");

            try
            {

                var citiesWeatherData = await _weatherService.GetData();
                await _weatherRepository.SaveData(citiesWeatherData);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



        }
    }
}
