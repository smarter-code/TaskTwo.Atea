using Newtonsoft.Json;

namespace Domain;

public class CityWeatherData
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int Cloudiness { get; set; }
    public DateTime LastUpdateTime { get; set; }

    //The partition key was chosen so that we store the data for each city for each month
    //Uniquely, this will make the queries for the data during the last two hours fast
    // As we will need to scan two partitions at most
    // We will also keep the partitions moderately sized, as each partition can have
    //at most 60 (the numbers of calls per hour) * 24 (number of hours of day) * 31 (maximum number of days in the month)
    // This is less than 45000
    public string Key => $"{Country}-{City}-{LastUpdateTime:yyyyMM}";
}