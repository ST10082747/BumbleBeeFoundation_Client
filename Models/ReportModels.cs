namespace BumbleBeeFoundation_Client.Models
{
    public class DonationReportItem
    {
        public int DonationID { get; set; }
        public DateTime DonationDate { get; set; }
        public string DonationType { get; set; }
        public decimal DonationAmount { get; set; }
        public string DonorName { get; set; }
        public string CompanyName { get; set; }
    }

    public class FundingRequestReportItem
    {
        public int RequestID { get; set; }
        public string CompanyName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectImpact { get; set; }
        public decimal RequestedAmount { get; set; }
        public string Status { get; set; }
        public string AdminMessage { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
