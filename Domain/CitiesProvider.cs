namespace Domain;

public static class CitiesProvider
{
    public static List<CityCountry> Cities { get; set; } = new()
    {
        new CityCountry("UK", "London"),
        new CityCountry("UK","Manchester"),
        new CityCountry("Sweden","Stockholm"),
        new CityCountry("Sweden","Gothenburg"),
        new CityCountry("Denmark","Copenhagen"),
        new CityCountry("Denmark","Aarhus"),
        new CityCountry("Norway","Oslo"),
        new CityCountry("Norway","Bergen"),
        new CityCountry("UAE","Dubai"),
        new CityCountry("UAE","Fujairah"),
    };
}