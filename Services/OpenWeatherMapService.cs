using Domain;
using Microsoft.Extensions.Options;
using Services.Extensions;
using Services.Interfaces;
using Shared;
using System.Security.Cryptography;
using System.Text;

namespace Services;

public class OpenWeatherMapService : IWeatherService
{
    private readonly HttpClient _client;
    private readonly OpenWeatherMapApiConfig _openWeatherMapApiConfig;
    public OpenWeatherMapService(IHttpClientFactory httpClientFactory
        , IOptions<OpenWeatherMapApiConfig> openWeatherMapApiConfig)
    {
        _openWeatherMapApiConfig = openWeatherMapApiConfig.Value;
        _client = httpClientFactory.CreateClient("OpenWeatherMapApiHttpClient");

    }
    public async Task<List<CityWeatherData>> GetData()
    {

        var currentListOfCities = CitiesProvider.Cities;

        var listOfWeatherData = new List<CityWeatherData>();
        foreach (var city in currentListOfCities)
        {
            var fullRoute = $"{_openWeatherMapApiConfig.WeatherGetRoute}{GetWeatherQueryString(city)}";
            var response = await _client.GetAsync(fullRoute);
            if (!response.IsSuccessStatusCode) //skip this execution if failed
            {
                continue;
            }
            var weatherResponse = await response.ReadContentAsJsonWithOptionAsync<WeatherResponse>();

            if (weatherResponse == null) //Something weird in the returned data format, skip this execution
            {
                continue;
            }

            //This unique key has been carefully chosen to avoid entering data duplicates
            //if we receive the same exact weather readings from the API (which is possible)
            //since the OpenWeatherAPI does update every minute like we do
            //This ensures that each item has a unique identifier based on the data you receive from the API.
            //If you receive duplicate data it will result on conflict and won't insert
            var uniqueId = CalculateMD5($"{city.CountryName}-{city.CityName}-{weatherResponse.dt}");
            var cityWeatherData = new CityWeatherData()
            {
                Id = uniqueId,
                Country = city.CountryName,
                City = city.CityName,
                Temperature = weatherResponse.main.temp,
                WindSpeed = weatherResponse.wind.speed,
                Cloudiness = weatherResponse.clouds.all,
                LastUpdateTime = (DateTimeOffset.FromUnixTimeSeconds(weatherResponse.dt)).UtcDateTime
            };
            listOfWeatherData.Add(cityWeatherData);
        }

        return listOfWeatherData;
    }


    private static string CalculateMD5(string input)
    {
        // Convert the input string to a byte array
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        // Create an MD5 instance and compute the hash
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to a hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
    private string GetWeatherQueryString(CityCountry city)
    {
        return $"?q={city.CityName},{city.CountryName}&units=metric&appid={_openWeatherMapApiConfig.ApiKey}";
    }
}