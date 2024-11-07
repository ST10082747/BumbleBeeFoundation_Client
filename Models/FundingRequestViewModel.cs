using System.ComponentModel.DataAnnotations;

namespace BumbleBeeFoundation_Client.Models
{
    public class FundingRequestViewModel
    {

        public int RequestID { get; set; }
        public int CompanyID { get; set; }
        public string? ProjectDescription { get; set; }
        public decimal RequestedAmount { get; set; }
        public string? ProjectImpact { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime SubmittedAt { get; set; }
        public string? CompanyName { get; set; }
        public List<AttachmentViewModel> Attachments { get; set; } = new List<AttachmentViewModel>();
        public string? AdminMessage { get; set; }
    }

    public class AttachmentViewModel
    {
        public int AttachmentID { get; set; }
        public int RequestID { get; set; }
        public string FileName { get; set; }
        public string BlobUrl { get; set; }
        public DateTime UploadedAt { get; set; }
    }


}
