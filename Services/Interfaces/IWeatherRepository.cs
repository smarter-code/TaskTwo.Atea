
using Domain;

namespace Services.Interfaces;

public interface IWeatherRepository
{
    Task SaveData(List<CityWeatherData> citiesWeatherData);

    Task<List<CityWeatherData>> GetTemperatureAndWindData(string cityKey, DateTime fromDate);

    Task<MinTempMaxWindData?> GetMinimum(string country, string city);

    Task Initialize();
}