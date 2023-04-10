using Domain;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Frontend.Controllers
{
    public class WeatherController : Controller
    {
        private readonly IWeatherRepository _weatherRepository;

        public WeatherController(IWeatherRepository weatherRepository)
        {
            _weatherRepository = weatherRepository;
        }
        [HttpGet]
        [Route("MinTempMaxWind")]
        [SwaggerOperation(Summary = "Get the minimum temperature and highest wind speed for all cities")]
        [SwaggerResponse(StatusCodes.Status200OK, "Values returned", typeof(List<MinTempMaxWindData>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An internal server error occurred")]
        public async Task<ActionResult<List<MinTempMaxWindData>>> MinTempMaxWind()
        {
            try
            {

                var listOfCitiesToGetWeatherData =
                    CitiesProvider.Cities;
                // Create a list of tasks to run in parallel to improve performance
                var tasks = listOfCitiesToGetWeatherData.Select(async item =>
                    await _weatherRepository.GetMinimum(item.CountryName, item.CityName)
                ).ToList();

                // Wait for all tasks to complete
                var result = await Task.WhenAll(tasks);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();
                Console.WriteLine($"Error Id: {errorId} An exception happened: {ex} ");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An internal server error occurred, please contact support with Erorr Id {errorId}");
            }

        }

        [HttpGet]
        [Route("LastTwoHoursData")]
        [SwaggerOperation(Summary = "Get the  data for the last two hours for a particular city in a country")]
        [SwaggerResponse(StatusCodes.Status200OK, "Values returned", typeof(List<CityWeatherData>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "An internal server error occurred")]
        public async Task<ActionResult<List<CityWeatherData>>> LastTwoHoursData(
            [FromQuery, SwaggerParameter("The country city key in the format Country-City ", Required = true)] string countryCity)
        {
            int hoursBefore = -2;
            if (string.IsNullOrEmpty(countryCity))
                return BadRequest("Country-City key is missing");
            try
            {

                var citiesWeatherData =
                    await _weatherRepository.GetTemperatureAndWindData(countryCity, DateTime.UtcNow.AddHours(hoursBefore));
                return Ok(citiesWeatherData);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();
                Console.WriteLine($"Error Id: {errorId} An exception happened: {ex} ");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An internal server error occurred, please contact support with Erorr Id {errorId}");
            }

        }

    }
}
