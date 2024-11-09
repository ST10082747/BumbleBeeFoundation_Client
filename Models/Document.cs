namespace BumbleBeeFoundation_Client.Models
{
    public class Document
    {
        public int DocumentID { get; set; }
        public string DocumentName { get; set; }
        public string DocumentType { get; set; }
        public DateTime UploadDate { get; set; }
        public string Status { get; set; }
        public string CompanyName { get; set; }

        public int CompanyID { get; set; }

        public byte[] FileContent { get; set; } // New field to store the file content
        public int FundingRequestID { get; set; } // Link to FundingRequest
    }
}
