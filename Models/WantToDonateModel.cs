using System.ComponentModel.DataAnnotations;

namespace BumbleBeeFoundation_Client.Models
{
    public class WantToDonateModel
    {
        [Required]
        [Display(Name = "Donor Name")]
        public required string DonorName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Donor Email")]
        public required string DonorEmail { get; set; }

        [Required]
        [Display(Name = "Donation Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid amount")]
        public decimal DonationAmount { get; set; }

        [Display(Name = "Donation Message")]
        public string? DonationMessage { get; set; }
    }
}
