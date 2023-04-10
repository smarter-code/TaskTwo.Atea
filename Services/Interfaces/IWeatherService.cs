using Domain;

namespace Services.Interfaces;

public interface IWeatherService
{
    Task<List<CityWeatherData>> GetData();
}