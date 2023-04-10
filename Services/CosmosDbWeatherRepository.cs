using Domain;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using Services.Extensions;
using Services.Interfaces;
using Shared;
using System.Net;

namespace Services;

public class CosmosDbWeatherRepository : IWeatherRepository
{
    private readonly string _endpointUri;
    private readonly string _primaryKey;
    private CosmosClient _cosmosClient;
    private Database _database;
    private Container _weatherDataContainer;
    private Container _minTempMaxWindContainer;
    private CosmosDbConfig _cosmosDbConfig;

    public CosmosDbWeatherRepository(IOptions<CosmosDbConfig> cosmosDbConfigOptions)
    {
        _cosmosDbConfig = cosmosDbConfigOptions.Value;
        _endpointUri = _cosmosDbConfig.EndpointUri;
        _primaryKey = _cosmosDbConfig.PrimaryKey;
        _cosmosClient = new CosmosClient(_endpointUri, _primaryKey);
    }
    public async Task SaveData(List<CityWeatherData> citiesWeatherData)
    {


        //Save weather entries to the database
        await SaveWeatherDataAsync(citiesWeatherData);

        //Update the minimum temperature and maximum wind table
        await UpdateMinTempMaxWind(citiesWeatherData);

    }

    //it is not possible to put in constructor due to async methods!
    public async Task Initialize()
    {
        //Create the database if does not exists
        await PrepareDb();
        // The container holding weather data if not exists
        await PrepareWeatherDataContainer();
        // The container holding the data for min temp and max wind for fast querying, instead of 
        //scanning the whole db. Create if not exists
        await PrepareMinTempMaxWindContainer();
    }

    public async Task<List<CityWeatherData>> GetTemperatureAndWindData(string cityKey, DateTime fromDate)
    {
        var toDateYearAndMonthAsInt = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var fromDateYearAndMonthAsInt = int.Parse(fromDate.ToString("yyyyMM"));
        // Define a limited the search range to avoid cross partition queries
        var partitionKeysSearchRange = new List<string>();
        for (int i = fromDateYearAndMonthAsInt; i <= toDateYearAndMonthAsInt; i++)
        {
            partitionKeysSearchRange.Add($"{cityKey}-{i}");
        }

        IOrderedQueryable<CityWeatherData> queryable = _weatherDataContainer
            .GetItemLinqQueryable<CityWeatherData>(requestOptions: new QueryRequestOptions { MaxConcurrency = -1 });
        var matches = queryable.Where(p => p.LastUpdateTime >= fromDate)
            .Where(p => partitionKeysSearchRange.Contains(p.Key));


        using FeedIterator<CityWeatherData> linqFeed = matches.ToFeedIterator();

        var results = new List<CityWeatherData>();

        while (linqFeed.HasMoreResults)
        {
            var response = await linqFeed.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task<MinTempMaxWindData?> GetMinimum(string country, string city)
    {

        IOrderedQueryable<MinTempMaxWindData> queryable = _minTempMaxWindContainer
            .GetItemLinqQueryable<MinTempMaxWindData>(requestOptions: new QueryRequestOptions { MaxConcurrency = -1, PartitionKey = new PartitionKey(country) });
        var matches = queryable.Where(p => p.Id == $"{country}-{city}");


        using FeedIterator<MinTempMaxWindData> linqFeed = matches.ToFeedIterator();

        var results = new List<MinTempMaxWindData>();

        while (linqFeed.HasMoreResults)
        {
            var response = await linqFeed.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results.FirstOrDefault();
    }

    private async Task PrepareMinTempMaxWindContainer()
    {
        await _database.CreateContainerIfNotExistsAsync(
            new ContainerProperties
            {
                Id = _cosmosDbConfig.MinTempMaxWindContainerName,
                PartitionKeyPath = $"/Key"
            });
        _minTempMaxWindContainer = _database.GetContainer(_cosmosDbConfig.MinTempMaxWindContainerName);
    }

    private async Task PrepareWeatherDataContainer()
    {

        await _database.CreateContainerIfNotExistsAsync(
            new ContainerProperties
            {
                Id = _cosmosDbConfig.WeatherDataContainerName,
                PartitionKeyPath = $"/Key"
            });
        _weatherDataContainer = _database.GetContainer(_cosmosDbConfig.WeatherDataContainerName);

    }

    private async Task UpdateMinTempMaxWind(List<CityWeatherData> citiesWeatherData)
    {
        foreach (var cityWeatherData in citiesWeatherData)
        {
            //Check if already have have the entry which holds the min temperature
            //and highest wind for each city
            //We need to fetch by the Key of MinTempMaxWindData which is Country-City
            var keyId = $"{cityWeatherData.Country}-{cityWeatherData.City}";
            var minTempMaxWindEntryFromDb = await GetMinTempMaxWindData(keyId, cityWeatherData.Country);
            if (minTempMaxWindEntryFromDb is null)
            {
                //There is no existin entry, Insert a new entry, we assume that the first entry we inserted
                //has the minimum temperature and highest wind speed, since we have not seen
                //something before it
                var minTempMaxWindEntry = new MinTempMaxWindData()
                {
                    City = cityWeatherData.City,
                    Country = cityWeatherData.Country,
                    MaximumWindSpeed = cityWeatherData.WindSpeed,
                    MinimumTemperature = cityWeatherData.Temperature,
                    LastUpdateTimeForMaximumWindSpeed = cityWeatherData.LastUpdateTime,
                    LastUpdateTimeForMinimumTemperature = cityWeatherData.LastUpdateTime
                };
                await SaveMinTempMaxWindDataAsync(minTempMaxWindEntry);
            }
            else
            {
                //A little optimization so that we do not hit the database unnecessarily
                bool updateFound = false;
                if (cityWeatherData.Temperature <= minTempMaxWindEntryFromDb.MinimumTemperature &&
                    //make sure it is a new value from the API (i.e. a more recent date)
                    cityWeatherData.LastUpdateTime > minTempMaxWindEntryFromDb.LastUpdateTimeForMinimumTemperature)
                {
                    updateFound = true;
                    //We found a new temperature value smaller than or equal to the previous one
                    //Let us update
                    minTempMaxWindEntryFromDb.MinimumTemperature = cityWeatherData.Temperature;
                    minTempMaxWindEntryFromDb.LastUpdateTimeForMinimumTemperature = cityWeatherData.LastUpdateTime;

                }
                if (cityWeatherData.WindSpeed >= minTempMaxWindEntryFromDb.MaximumWindSpeed &&
                    //make sure it is a new value from the API (i.e. a more recent date)
                    cityWeatherData.LastUpdateTime > minTempMaxWindEntryFromDb.LastUpdateTimeForMaximumWindSpeed)
                {
                    //We found a new temperature value smaller than or equal to the previous one
                    //Let us update
                    updateFound = true;
                    minTempMaxWindEntryFromDb.MaximumWindSpeed = cityWeatherData.WindSpeed;
                    minTempMaxWindEntryFromDb.LastUpdateTimeForMaximumWindSpeed = cityWeatherData.LastUpdateTime;
                }
                if (updateFound)
                    await UpdateMinTempMaxWindDataAsync(minTempMaxWindEntryFromDb);
            }
        }
    }

    private async Task PrepareDb()
    {
        _cosmosClient = new CosmosClient(_endpointUri, _primaryKey);
        _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_cosmosDbConfig.WeatherDatabaseName);
    }

    private async Task SaveMinTempMaxWindDataAsync(MinTempMaxWindData minTempMaxWindData)
    {
        try
        {

            ItemResponse<MinTempMaxWindData> response = await _minTempMaxWindContainer.CreateItemAsync(minTempMaxWindData, new PartitionKey(minTempMaxWindData.Key));
            Console.WriteLine($"Data saved successfully. Item Id: {response.Resource.Id}");
        }
        catch (CosmosException ex)
        {
            //Something went wrong, log the error
            Console.WriteLine($"Error saving data. StatusCode: {ex.StatusCode}, Message: {ex.Message}");
        }


    }

    private async Task UpdateMinTempMaxWindDataAsync(MinTempMaxWindData minTempMaxWindData)
    {
        try
        {

            ItemResponse<MinTempMaxWindData> response = await _minTempMaxWindContainer.ReplaceItemAsync(minTempMaxWindData, minTempMaxWindData.Key, new PartitionKey(minTempMaxWindData.Key));
            Console.WriteLine($"Data updated successfully. Item Id: {response.Resource.Id}");
        }
        catch (CosmosException ex)
        {
            //Something went wrong, log the error
            Console.WriteLine($"Error updating data. StatusCode: {ex.StatusCode}, Message: {ex.Message}");
        }


    }
    private async Task SaveWeatherDataAsync(List<CityWeatherData> citiesWeatherData)
    {
        foreach (var cityWeatherData in citiesWeatherData)
        {
            try
            {

                ItemResponse<CityWeatherData> response = await _weatherDataContainer.CreateItemAsync(cityWeatherData, new PartitionKey(cityWeatherData.Key));
                Console.WriteLine($"Data saved successfully. Item Id: {response.Resource.Id}");
            }
            catch (CosmosException ex)
            {

                Console.WriteLine($"Error saving data. StatusCode: {ex.StatusCode}, Message: {ex.Message}");
            }
        }

    }
    private async Task<MinTempMaxWindData?> GetMinTempMaxWindData(string id, string partitionKey)
    {
        try
        {
            var readResponse = await _minTempMaxWindContainer.ReadItemStreamAsync(
                id: id,
                partitionKey: new PartitionKey(partitionKey)
            );

            if (readResponse.StatusCode == HttpStatusCode.NotFound)
                return null;
            if (readResponse.StatusCode == HttpStatusCode.OK)
            {
                return await readResponse.ReadContentAsJsonAsync<MinTempMaxWindData>();
            }

            throw new Exception($"Something wrong happened at Cosmos for GetMinTempMaxWind with id {id}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }




}