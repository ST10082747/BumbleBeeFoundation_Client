using System.ComponentModel.DataAnnotations;

namespace BumbleBeeFoundation_Client.Models
{
    public class Company
    {
        public int CompanyID { get; set; }

        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; }

        [Required]
        public string Description { get; set; }

        [Display(Name = "Date Joined")]
        public DateTime DateJoined { get; set; }

        public string Status { get; set; }

        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }
    }
}
