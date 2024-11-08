using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace BumbleBeeFoundation_Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    public class PayFastRequest
    {
        public string merchant_id { get; set; }
        public string merchant_key { get; set; }
        public string return_url { get; set; }
        public string cancel_url { get; set; }
        public string notify_url { get; set; }
        public string name_first { get; set; }
        public string email_address { get; set; }
        public string m_payment_id { get; set; }
        public string amount { get; set; }
        public string item_name { get; set; }
        public string payment_method { get; set; }

        public string GenerateSignature(string passPhrase)
        {
            // Create dictionary of all properties
            var dataArray = GetType().GetProperties()
                .Where(p => p.GetValue(this) != null)
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(this).ToString()
                );

            // Create parameter string
            var pfOutput = "";
            foreach (var item in dataArray)
            {
                if (!string.IsNullOrEmpty(item.Value))
                {
                    // Replace spaces with + and URL encode
                    var encodedValue = HttpUtility.UrlEncode(item.Value.Trim()).Replace("%20", "+");
                    pfOutput += $"{item.Key}={encodedValue}&";
                }
            }

            // Remove last ampersand
            var getString = pfOutput.TrimEnd('&');

            // Add passphrase if provided
            if (!string.IsNullOrEmpty(passPhrase))
            {
                getString += $"&passphrase={HttpUtility.UrlEncode(passPhrase.Trim()).Replace("%20", "+")}";
            }

            // For debugging
            System.Diagnostics.Debug.WriteLine($"Final string to hash: {getString}");

            // Generate MD5 hash
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(getString));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }


    }


}
