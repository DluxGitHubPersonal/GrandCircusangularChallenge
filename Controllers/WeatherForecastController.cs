using Microsoft.AspNetCore.Mvc;

namespace GrandCircusAspWithAngular.Controllers
{
    [ApiController]
    [Route("{controller}/{action=Index}")]
    public class WeatherForecastController : ControllerBase
    {
        #region Public Methods
        [HttpGet]
        public string Text()
        {
            return "Hello world";
        }

        /// <summary>
        /// Get the current weather for the city best matching the specified name anywhere in the world.
        /// </summary>
        /// <param name="cityName">Name of city to find.  Must not include state or country.</param>
        /// <returns>Weather for that city, or data == null if city could not be identified or located</returns>
        [HttpGet]
        public async Task<ServiceResponse<CityWeatherInfo>> GetBestGuessCityCurrentWeather( string cityName )
        {
            var response = new ServiceResponse<CityWeatherInfo>();
            response.succeeded = false;
            response.failureMessage = "Not yet initialized";

            try
            {
                if (weatherClient != null)
                {
                    // Get the best match by city name only
                    var cityLocation = await weatherClient.GetBestCityLocation(cityName, "", "");
                    response.succeeded = true;
                    if (cityLocation != null)
                    {
                        var cityCurrentWeather = await weatherClient.GetCurrentWeatherByLocation(cityLocation.lattitude, cityLocation.longitude, OpenWeatherClient.Units.Imperial);
                        if (cityCurrentWeather != null)
                        {
                            response.data = new CityWeatherInfo(cityCurrentWeather, OpenWeatherClient.Units.Imperial, cityLocation.state, cityLocation.country);
                            response.failureMessage = null;
                        }
                        else
                        {
                            // Partial success - treat as unable to locate city, but provide a failureMessage for developer
                            response.failureMessage = $"Unable to get current weather for city {cityName}";
                        }
                    }
                    else
                    {
                        // Could not locate city - API succeess with no data returned, failureMessage provided to assist developer
                        response.succeeded = true;
                        response.failureMessage = $"Unable to locate city {cityName}";
                    }
                }
                else
                {
                    // This should never happen.
                    response.failureMessage = "Unable to obtain an OpenWeather API client.  Weather services are offline";
                }
            }
            catch (Exception unexpectedException)
            {
                response.failureMessage = $"Error {unexpectedException.GetType().Name}:  {unexpectedException.Message}";
            }

            return response;
        }
        #endregion Public Methods

        #region Public Classes
        /// <summary>
        /// The basic type for any service response
        /// SYNCHRONIZE WITH JAVASCRIPT weather.service.ts|ServiceResponse[T]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class ServiceResponse<T>
            where T : class, new()
        {
            /// <summary>
            /// TRUE on success, FALSE on any failure
            /// </summary>
            public bool                             succeeded                   { get; set; }
            /// <summary>
            /// Service provided message describing why the API failed.
            /// </summary>
            public string?                          failureMessage              { get; set; }
            /// <summary>
            /// The result provided by the API.  May also be set in success/no data cases to assist developers.
            /// </summary>
            public T?                               data                        { get; set; }
        }

        /// <summary>
        /// Describes the weather of a city
        /// SYNCHRONIZE WITH JAVASCRIPT weather.service.ts|getBestGuessCityWeatherResult
        /// </summary>
        public class CityWeatherInfo
        {
            #region Public Properties
            public string?                          cityName                        { get; set; }
            public string?                          stateName                       { get; set; }
            public string?                          countryId                       { get; set; }
            public ulong                            cityCode                        { get; set; }
            public string?                          conditionsDescription           { get; set; }
            public int                              conditionsStandardId            { get; set; }
            public decimal                          temperatureActualFarenheit      { get; set; }
            public decimal                          temperatureFeelsLikeFarenheit   { get; set; }
            public decimal                          currentWindSpeedMph             { get; set; }
            #endregion Public Properties

            #region Constructor
            public CityWeatherInfo( OpenWeatherClient.WeatherForLocationResult newSourceResult, OpenWeatherClient.Units newSourceResultUnits, string newStateName, string newCountryId )
            {
                const decimal kilometersPerMile = 1.609344M;

                // Validate parameters
                if (newSourceResult == null)
                    throw new ArgumentNullException(nameof(newSourceResult));

                cityName = newSourceResult.cityName;
                stateName = newStateName;
                countryId = newCountryId;
                cityCode = newSourceResult.cityId;
                if (newSourceResult.conditions?.Length > 0)
                {
                    conditionsDescription = newSourceResult.conditions[0].conditionDescription;
                    conditionsStandardId = newSourceResult.conditions[0].conditionStandardId;
                }
                if (newSourceResultUnits == OpenWeatherClient.Units.Standard)
                {
                    temperatureActualFarenheit = KelvinToFarenheit((newSourceResult.temperature?.actualTemperature).GetValueOrDefault());
                    temperatureFeelsLikeFarenheit = KelvinToFarenheit((newSourceResult.temperature?.feelsLikeTemperature).GetValueOrDefault());
                    currentWindSpeedMph = ((newSourceResult.wind?.averageSpeed / kilometersPerMile).GetValueOrDefault()/kilometersPerMile);
                }
                else if (newSourceResultUnits == OpenWeatherClient.Units.Metric)
                {
                    temperatureActualFarenheit = CelsiusToFarenheit((newSourceResult.temperature?.actualTemperature).GetValueOrDefault());
                    temperatureFeelsLikeFarenheit = CelsiusToFarenheit((newSourceResult.temperature?.feelsLikeTemperature).GetValueOrDefault());
                    currentWindSpeedMph = (newSourceResult.wind?.averageSpeed / kilometersPerMile).GetValueOrDefault();
                }
                else if (newSourceResultUnits == OpenWeatherClient.Units.Imperial)
                {
                    temperatureActualFarenheit = (newSourceResult.temperature?.actualTemperature).GetValueOrDefault();
                    temperatureFeelsLikeFarenheit = (newSourceResult.temperature?.feelsLikeTemperature).GetValueOrDefault();
                    currentWindSpeedMph = (newSourceResult.wind?.averageSpeed).GetValueOrDefault(); ;
                }
            }

            /// <summary>
            /// Converts degrees Celsius to degrees Farenheit
            /// </summary>
            /// <param name="degreesFarenheit"></param>
            /// <returns></returns>
            private decimal CelsiusToFarenheit( decimal degreesFarenheit )
            {
                const decimal farenheitDegreesPerCelsius = 1.8M;

                return degreesFarenheit * farenheitDegreesPerCelsius + 32;
            }

            /// <summary>
            /// Converts degrees Kelvin to degrees Farenheit
            /// </summary>
            /// <param name="degreesFarenheit"></param>
            /// <returns></returns>
            private decimal KelvinToFarenheit( decimal degreesFarenheit )
            {
                const decimal farenheitDegreesPerCelsius = 1.8M;

                return degreesFarenheit * farenheitDegreesPerCelsius - 459.67M;
            }

            public CityWeatherInfo()
            {
                // Nothing needed
            }
            #endregion Constructor
        }
        #endregion Public Classes

        #region Constructor
        public WeatherForecastController(OpenWeatherClient newWeatherClient, ILogger<WeatherForecastController> newLogger)
        {
            logger = newLogger;
            weatherClient = newWeatherClient;
        }
        #endregion Constructor

        #region Private Data Members
        private readonly        ILogger<WeatherForecastController>          logger;
        private readonly        OpenWeatherClient                           weatherClient;
        private const           string                                      OpenWeatherMapApiKey    = "01f9874beb4c134f2949c5c6b9a80b6a";
        #endregion Private Data Members
    }
}