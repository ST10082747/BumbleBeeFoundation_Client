using System.ComponentModel.DataAnnotations;

namespace BumbleBeeFoundation_Client.Models
{
    public class Donation
    {
        public int DonationID { get; set; }

        [Display(Name = "Company")]
        public int? CompanyID { get; set; }

        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }

        [Required]
        [Display(Name = "Donation Date")]
        [DataType(DataType.Date)]
        public DateTime DonationDate { get; set; }

        [Required]
        [Display(Name = "Donation Type")]
        public string DonationType { get; set; }

        [Required]
        [Display(Name = "Donation Amount")]
        [DataType(DataType.Currency)]
        public decimal DonationAmount { get; set; }

        [Required]
        [Display(Name = "Donor Name")]
        public string DonorName { get; set; }

        [Required]
        [Display(Name = "Donor ID Number")]
        public string DonorIDNumber { get; set; }

        [Required]
        [Display(Name = "Donor Tax Number")]
        public string DonorTaxNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Donor Email")]
        public string DonorEmail { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Donor Phone")]
        public string DonorPhone { get; set; }

        public string PaymentStatus { get; set; }

        public byte[]? DocumentPath { get; set; }
        public string? DocumentFileName { get; set; }

    }
}