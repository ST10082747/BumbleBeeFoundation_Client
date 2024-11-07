namespace BumbleBeeFoundation_Client.Models
{
    public class CompanyViewModel
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Description { get; set; }
        public DateTime DateJoined { get; set; }
        public string Status { get; set; } // E.g., Active, Inactive, Pending, etc.

        public string RejectionReason { get; set; }


    }

}
