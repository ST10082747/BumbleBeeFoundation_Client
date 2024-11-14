using Newtonsoft.Json.Linq;

namespace BumbleBeeFoundation_Client.Services
{
    // Service for converting currencies to rands
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<CurrencyConverterService> _logger;

        public CurrencyConverterService(HttpClient httpClient, ILogger<CurrencyConverterService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = Environment.GetEnvironmentVariable("CURRENCY_API_KEY");

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("API Key for Currency API is not set in environment variables. Currency conversion will not function properly.");
            }
        }

        public async Task<decimal?> ConvertToRand(decimal amount, string currencyCode)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("Attempted currency conversion without a valid API key. Operation aborted.");
                return null; 
            }

            try
            {
                var url = $"https://api.currencyapi.com/v3/latest?apikey={_apiKey}&base_currency={currencyCode}&currencies=ZAR";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var conversionRate = (decimal)json["data"]["ZAR"]["value"];
                return amount * conversionRate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while converting currency from {CurrencyCode} to ZAR", currencyCode);
                return null;
            }
        }
    }


}
