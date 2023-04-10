namespace Shared
{
    public class CosmosDbConfig
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string WeatherDataContainerName { get; set; }
        public string MinTempMaxWindContainerName { get; set; }

        public string WeatherDatabaseName { get; set; }
    }
}
