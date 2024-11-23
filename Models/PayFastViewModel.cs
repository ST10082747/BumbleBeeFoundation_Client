namespace BumbleBeeFoundation_Client.Models
{
    public class PayFastViewModel
    {
        public string MerchantId { get; set; }
        public string MerchantKey { get; set; }
        public string NameFirst { get; set; }
        public string EmailAddress { get; set; }
        public string PaymentId { get; set; }
        public string Amount { get; set; }
        public string ItemName { get; set; }
        public string PaymentMethod { get; set; }
        public string Signature { get; set; }
        public string PayFastUrl { get; set; }
    }
}
