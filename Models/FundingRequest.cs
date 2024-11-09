using System.ComponentModel.DataAnnotations;

namespace BumbleBeeFoundation_Client.Models
{
    public class FundingRequest
    {
        public int RequestID { get; set; }

        [Required]
        public int CompanyID { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Project Description")]
        public string ProjectDescription { get; set; }

        [Required]
        [Display(Name = "Requested Amount")]
        [DataType(DataType.Currency)]
        public decimal RequestedAmount { get; set; }

        [Required]
        [Display(Name = "Project Impact")]
        public string ProjectImpact { get; set; }

        [Required]
        public string Status { get; set; }

        [Display(Name = "Submitted At")]
        public DateTime SubmittedAt { get; set; }

        public string AdminMessage { get; set; }
    }
}
