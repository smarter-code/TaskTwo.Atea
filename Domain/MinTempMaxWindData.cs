using Newtonsoft.Json;

namespace Domain;

public class MinTempMaxWindData
{
    [JsonProperty(PropertyName = "id")]
    public string Id => $"{Country}-{City}";
    public string Country { get; set; }
    public string City { get; set; }
    public double MinimumTemperature { get; set; }
    public double MaximumWindSpeed { get; set; }
    public DateTime LastUpdateTimeForMinimumTemperature { get; set; }

    public DateTime LastUpdateTimeForMaximumWindSpeed { get; set; }

    //The partition key was chosen to make the queries to get the minimum temperature
    //or maximum wind speed very fast, we will need to hit one partition only this way
    public string Key => $"{Country}";
}