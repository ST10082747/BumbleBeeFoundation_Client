namespace BumbleBeeFoundation_Client
{
    // Obfuscation used to hide the ID and tax numbers in the DonationDetails view for admin
    public static class ObfuscationHelper
    {
        public static string Obfuscate(string value, int visibleLength = 4)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= visibleLength)
                return value;

            return new string('*', value.Length - visibleLength) + value.Substring(value.Length - visibleLength);
        }
    }

}
