namespace BumbleBeeFoundation_Client.Services
{
    // Interface for the currency conversion service
    public interface ICurrencyConverterService
    {
        Task<decimal?> ConvertToRand(decimal amount, string currencyCode);
    }


}
