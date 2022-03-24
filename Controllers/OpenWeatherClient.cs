using System.Text.Json.Serialization;

namespace GrandCircusAspWithAngular.Controllers
{
    public class OpenWeatherClient
    {
        #region Private Data Members
        private HttpClient                  httpClient;
        private ILogger<OpenWeatherClient>? logger;

        private const string OpenWeatherMapBaseUri                  = "http://api.openweathermap.org";
        private const string OpenWeatherMapApi_CityNameInfo         = "geo/1.0/direct";
        private const string OpenWeatherMapApi_WeatherForLocation   = "data/2.5/weather";
        private const string OpenWeatherMapApiKey                   = "01f9874beb4c134f2949c5c6b9a80b6a";
        #endregion Private Data Members

        #region Constructor
        public OpenWeatherClient( IHttpClientFactory newHttpClientFactory, ILogger<OpenWeatherClient> newLogger )
        {
            // Validate parameters
            if (newHttpClientFactory == null)
                throw new ArgumentNullException(nameof(newHttpClientFactory));

            httpClient = newHttpClientFactory.CreateClient();
            logger = newLogger;
        }

        /// <summary>
        /// For test only.
        /// </summary>
        /// <param name="newHttpClient"></param>
        /// <exception cref="ArgumentNullException">newHttpClient NULL</exception>
        private OpenWeatherClient( HttpClient newHttpClient, ILogger<OpenWeatherClient> newLogger )
        {
            // Validate parameters
            if (newHttpClient == null)
                throw new ArgumentNullException(nameof(newHttpClient));

            httpClient = newHttpClient;
            logger = newLogger;
        }
        #endregion Constructor

        #region Public Methods
        /// <summary>
        /// Gets the location of the specified city from OpenWeatherMap.org with a 5 second timeout.
        /// </summary>
        /// <param name="cityName">Name of the city</param>
        /// <param name="stateName">Name of the state</param>
        /// <param name="countryName">Name of country</param>
        /// <returns>Weather data for the specified city</returns>
        /// <remarks>
        /// Due to varying expressions of names, the result may not be for the city specified.  To eliminate or reduce
        /// uncertainty, use the same city, state, and country names as OpenWeatherMap.org service does.  For example,
        /// use full state names instead of abbreviations, and use US for the United States.  This API returns the best
        /// matching result from the OpenWeatherMap results.
        /// </remarks>
        /// <exception cref="Timeout">OpenWeatherMap.org does not respond within 5 seconds</exception>
        /// <exception cref="System.Text.Json.JsonException">OpenWeatherMap response not recognized in some cases</exception>
        /// <exception cref="*">Any other error</exception>
        /// <exception cref="ArgumentNullException">Any parameter NULL</exception>
        public async Task<GeoResult?> GetBestCityLocation(string cityName, string stateName, string countryName)
        {
            // Validate parameters
            if (String.IsNullOrWhiteSpace(cityName))
                throw new ArgumentNullException(nameof(cityName));

            var bestResult = (GeoResult?) null;

            // Decode city/state/country name to longitude/lattitude.  Max results 5.
            string requestUri = $"{OpenWeatherMapBaseUri}/{OpenWeatherMapApi_CityNameInfo}?q={cityName},{stateName},{countryName}&limit=5&appid={OpenWeatherMapApiKey}";
            var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var apiResult = await httpClient.GetStringAsync(requestUri, cancelSource.Token);
            if (apiResult != null)
            {
                var parsedResult = System.Text.Json.JsonSerializer.Deserialize<GeoResult[]>(apiResult);

                if (parsedResult != null)
                {
                    // Find the best match.  OpenWeatherMap returns "best match" data in several ways:
                    // 1.  All fields support aliases.  For example, it recognizes both MI and Michigan, and US, USA, and United States as intended.
                    // 2.  Results are returned using normalized names instead of parameter matches.  For example, a call with parameters
                    //     Detroit, MI, USA will result in a top match record described using Detroit, Michigan, US instead.
                    // 3.  If specifies do not match OpenWeatherMap canonical names, the service will provide many additional unexpected results.
                    //     For example, a call with Detroit, MI, US provides results for Detroit, IL and Detroit, OR, among others.
                    //     But a call with Detroit, MI, USA - note USA instead of US - has Le Detroit, Normandy, FR as the second highest result.
                    var exactMatches = parsedResult.Where(x => x.name.EqualsNoCase(cityName) && x.state.EqualsNoCase(stateName) && x.country.EqualsNoCase(countryName)).ToArray();
                    var cityStateMatches = parsedResult.Where(x => x.name.EqualsNoCase(cityName) && x.state.EqualsNoCase(stateName)).ToArray();
                    var cityOnlyMatches = parsedResult.Where(x => x.name.EqualsNoCase(cityName)).ToArray();
                    if (exactMatches.Length > 0)
                        bestResult = exactMatches[0];
                    else if (cityStateMatches.Length > 0)
                        bestResult = cityStateMatches[0];
                    else if (cityOnlyMatches.Length > 0)
                        bestResult = cityOnlyMatches[0];
                }
            }
            return bestResult;
        }

        public async Task<WeatherForLocationResult?> GetCurrentWeatherByLocation( decimal lattitude, decimal longitude, Units units = Units.Metric )
        {
            string requestUri = $"{OpenWeatherMapBaseUri}/{OpenWeatherMapApi_WeatherForLocation}?lat={lattitude}&lon={longitude}&units={units}&appid={OpenWeatherMapApiKey}";
            var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            string? apiResultString = null;

            apiResultString = await httpClient.GetStringAsync(requestUri, cancelSource.Token);
            var parsedResult = System.Text.Json.JsonSerializer.Deserialize<WeatherForLocationResult>(apiResultString);
            return parsedResult;
        }

        /// <summary>
        /// Gets the weather for a city using it's well-known integer identifier.  These are available from OpenWeather as JSON files.
        /// </summary>
        /// <param name="wellKnownCityId">The cities fixed identifier</param>
        /// <param name="units">Units to use for result</param>
        /// <returns>The city's current weather</returns>
        public async Task<WeatherForLocationResult?> GetCurrentWeatherForCityById( int wellKnownCityId, Units units = Units.Metric )
        {
            string requestUri = $"{OpenWeatherMapBaseUri}/{OpenWeatherMapApi_WeatherForLocation}?id={wellKnownCityId}&units={units}&appid={OpenWeatherMapApiKey}";
            var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            string? apiResultString = null;

            apiResultString = await httpClient.GetStringAsync(requestUri, cancelSource.Token);
            var parsedResult = System.Text.Json.JsonSerializer.Deserialize<WeatherForLocationResult>(apiResultString);
            return parsedResult;
        }
        #endregion Public Methods

        #region Public Classes and Types
        public enum Units
        {
            /// <summary>
            /// Kelvin, Default.
            /// </summary>
            Standard,
            /// <summary>
            /// Celsius
            /// </summary>
            Metric,
            /// <summary>
            /// Farenheit
            /// </summary>
            Imperial
        }

        /// <summary>
        /// Result from OpenWeatherMap Geo API that determines the lattitude and longitude of a city by name
        /// </summary>
        public class GeoResult
        {
            public string?                      name                        { get; set; }
            [JsonPropertyName("local_names")]
            public Dictionary<string, string>?  languageToName              { get; set; }
            [JsonPropertyName("lat")]
            public decimal                      lattitude                   { get; set; }
            [JsonPropertyName("lon")]
            public decimal                      longitude                   { get; set; }
            public string?                      country                     { get; set; }
            public string?                      state                       { get; set; }
        }

        public class WeatherForLocationResult
        {
            [JsonPropertyName("coord")]
            public GeoCoordinate?               location                    { get; set; }
            [JsonPropertyName("weather")]
            public ConditionDescription[]?      conditions                  { get; set; }
            [JsonPropertyName("base")]
            public string?                      source                      { get; set; }
            [JsonPropertyName("main")]
            public TemperatureDescription?      temperature                 { get; set; }
            [JsonPropertyName("visibility")]
            public decimal                      visibilityDistance          { get; set; }
            [JsonPropertyName("wind")]
            public WindDescription?             wind                        { get; set; }
            [JsonPropertyName("clouds")]
            public CloudDescription?            cloud                       { get; set; }
            [JsonPropertyName("dt")]
            public ulong                        dataCalculationUnixTime     { get; set; }
            [JsonPropertyName("sys")]
            public SunriseDescription?          sunrise                     { get; set; }
            [JsonPropertyName("timezone")]
            public long                         timezoneUtcOffsetSeconds    { get; set; }
            [JsonPropertyName("id")]
            public ulong                        cityId                      { get; set; }
            [JsonPropertyName("name")]
            public string?                      cityName                    { get; set; }
            /// <summary>
            /// Undocumented
            /// </summary>
            [JsonPropertyName("cod")]
            public int                          cod                         { get; set; }

        }
        public class SunriseDescription
        {
            /// <summary>
            /// Undocumented
            /// </summary>
            [JsonPropertyName("type")]
            public int                          type                        { get; set; }
            /// <summary>
            /// Undocumented
            /// </summary>
            [JsonPropertyName("id")]
            public int                          id                          { get; set; }
            [JsonPropertyName("country")]
            public string?                      country                     { get; set; }
            [JsonPropertyName("sunrise")]
            public ulong                        sunriseTimeUnix             { get; set; }
            [JsonPropertyName("sunset")]
            public ulong                        sunsetTimeUnix              { get; set; }
        }

        public class CloudDescription
        {
            [JsonPropertyName("all")]
            public decimal                      percentCloudy               { get; set; }
        }

        public class WindDescription
        {
            [JsonPropertyName("speed")]
            public decimal                      averageSpeed                { get; set; }
            [JsonPropertyName("deg")]
            public decimal                      direction                   { get; set; }
            [JsonPropertyName("gust")]
            public decimal                      gustSpeed                   { get; set; }
        }

        /// <summary>
        /// A summary of the overall conditions suitable for human use - Misty, rainy, etc.
        /// </summary>
        public class ConditionDescription
        {
            [JsonPropertyName("id")]
            public int                          conditionStandardId         { get; set; }
            [JsonPropertyName("main")]
            public string?                      conditionType               { get; set; }
            [JsonPropertyName("description")]
            public string?                      conditionDescription        { get; set; }
            [JsonPropertyName("icon")]
            public string?                      displayIconId               { get; set; }
        }

        public class TemperatureDescription
        {
            [JsonPropertyName("temp")]
            public decimal                      actualTemperature           { get; set; }
            [JsonPropertyName("feels_like")]
            public decimal                      feelsLikeTemperature        { get; set; }
            [JsonPropertyName("temp_min")]
            public decimal                      observedMinimum             { get; set; }
            [JsonPropertyName("temp_max")]
            public decimal                      observedMaximum             { get; set; }
            [JsonPropertyName("pressure")]
            public decimal                      atmosphericPressure         { get; set; }
            [JsonPropertyName("humidity")]
            public decimal                      humidity                    { get; set; }
        }

        /// <summary>
        /// Goegraphic coordinates.  NET core does not support System.Device.GeoCoordinate.
        /// </summary>
        public class GeoCoordinate
        {
            [JsonPropertyName("lon")]
            public decimal                      lattitude                   { get; set; }
            [JsonPropertyName("lat")]
            public decimal                      longitude                   { get; set; }
        }
        #endregion Public Classes and Types
    }

    public static class Extensions
    {
        public static bool EqualsNoCase(this string firstArg, string secondArg)
        {
            return String.Equals(firstArg, secondArg, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}