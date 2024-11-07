using System.ComponentModel.DataAnnotations;

namespace BumbleBeeFoundation_Client.Models
{
    public class WantToBenefitModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public required string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public required string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Project Description")]
        public required string ProjectDescription { get; set; }

        [Required]
        [Display(Name = "Requested Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid amount")]
        public decimal RequestedAmount { get; set; }

        [Required]
        [Display(Name = "Impact of the Project")]
        public required string ProjectImpact { get; set; }
    }
}
