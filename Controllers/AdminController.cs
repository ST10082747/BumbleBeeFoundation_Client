using BumbleBeeFoundation_Client.Models;
using BumbleBeeFoundation_Client.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BumbleBeeFoundation_Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IEmailService _emailService;
        private readonly CertificateService _certificateService;

        public AdminController(
            ILogger<AdminController> logger,
            IHttpClientFactory httpClientFactory, IEmailService emailService,
        CertificateService certificateService)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
            _emailService = emailService;
            _certificateService = certificateService;
        }

        // Dashboard Action - Fetches statistics from the API for the admin
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/dashboard");
                response.EnsureSuccessStatusCode(); 

                var json = await response.Content.ReadAsStringAsync();
                var dashboardViewModel = JsonConvert.DeserializeObject<DashboardViewModel>(json);

                return View(dashboardViewModel);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Error fetching dashboard data: {e.Message}");
                return View("Error");
            }
        }

        // User Management actions

        // User Management - Fetches users from the API for the admin
        public async Task<IActionResult> UserManagement()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/users");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(json);

                return View(users);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Error fetching users: {e.Message}");
                return View("Error");
            }
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Create User - Let an admin send data to the API to create a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/admin/users", user);
                    response.EnsureSuccessStatusCode();

                    return RedirectToAction(nameof(UserManagement));
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError($"Error creating user: {e.Message}");
                    return View("Error");
                }
            }
            return View(user);
        }

        // Edit User - Fetches the user details to edit, from the API
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/admin/users/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(json);

               
                var userForEdit = new UserForEdit
                {
                    UserID = user.UserID,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role
                };

                return View(userForEdit);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Error fetching user for editing: {e.Message}");
                return View("Error");
            }
        }


        // POST: Edit User - Allow admin to send the updated user data to the API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, UserForEdit userForEdit)
        {
            if (id != userForEdit.UserID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{id}", userForEdit);
                    response.EnsureSuccessStatusCode();

                    return RedirectToAction(nameof(UserManagement));
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError($"Error updating user: {e.Message}");
                    return View("Error");
                }
            }
            return View(userForEdit);
        }

        // Delete User - Allow admin to delete the user
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/admin/users/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(json);

                return View(user);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Error fetching user for deletion: {e.Message}");
                return View("Error");
            }
        }

        // POST: Delete User - Send delete request to the API
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/admin/users/{id}");
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(UserManagement));
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Error deleting user: {e.Message}");
                return View("Error");
            }
        }


        // Company Management actions

        // Fetch list of all companies in the database, using the API

        // GET: /Admin/CompanyManagement
        public async Task<IActionResult> CompanyManagement()
        {
            var companies = new List<Company>();

            try
            {
                var response = await _httpClient.GetAsync("api/admin/companies");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    companies = JsonConvert.DeserializeObject<List<Company>>(json);
                }
                else
                {
                    _logger.LogError("Failed to retrieve companies: {StatusCode}", response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error while fetching companies from API.");
            }

            return View(companies);
        }

        // Get details for a specific company

        // GET: /Admin/CompanyDetails/{id}
        public async Task<IActionResult> CompanyDetails(int id)
        {
            Company company = null;

            try
            {
                var response = await _httpClient.GetAsync($"api/admin/companies/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    company = JsonConvert.DeserializeObject<Company>(json);
                }
                else
                {
                    _logger.LogWarning("Company with ID {id} not found", id);
                    return NotFound();
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error while fetching company details from API.");
            }

            return View(company);
        }

        // Allow the admin to approve a company

        // POST: /Admin/ApproveCompany/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCompany(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/admin/companies/approve/{id}", null);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to approve company with ID {id}: {StatusCode}", id, response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error while approving company via API.");
            }

            return RedirectToAction(nameof(CompanyManagement));
        }

        // Allow the admin to reject a company

        // POST: /Admin/RejectCompany/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectCompany(int id, string rejectionReason)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(rejectionReason);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"api/admin/companies/reject/{id}", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to reject company with ID {id}: {StatusCode}", id, response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error while rejecting company via API.");
            }

            return RedirectToAction(nameof(CompanyManagement));
        }



        // Donation Management actions

        // Use the API to fetch list of donations in the database
        public async Task<IActionResult> DonationManagement()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/donations");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var donations = JsonConvert.DeserializeObject<List<Donation>>(content);
                    return View(donations);
                }
                else
                {
                    _logger.LogError("Failed to retrieve donations. Status code: {StatusCode}", response.StatusCode);
                    TempData["ErrorMessage"] = "Failed to retrieve donations.";
                    return View(new List<Donation>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving donations");
                TempData["ErrorMessage"] = "An error occurred while retrieving donations.";
                return View(new List<Donation>());
            }
        }

        // Allow the admin to approve a donation

        [HttpPost]
        public async Task<IActionResult> ApproveDonation(int id)
        {
            try
            {
                // Approve the donation through the API
                var response = await _httpClient.PutAsync($"api/admin/donations/{id}/approve", null);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to approve donation.";
                    return RedirectToAction(nameof(DonationManagement));
                }

                // Get the updated donation details
                var donationResponse = await _httpClient.GetAsync($"api/admin/donations/{id}");
                if (donationResponse.IsSuccessStatusCode)
                {
                    var content = await donationResponse.Content.ReadAsStringAsync();
                    var donation = JsonConvert.DeserializeObject<Donation>(content);

                    if (donation != null)
                    {
                        // Generate certificate
                        var certificatePdf = _certificateService.GenerateDonationCertificate(donation);

                        // Send email with section 18 certificate
                        await _emailService.SendDonationCertificateAsync(
                            donation.DonorEmail,
                            donation.DonorName,
                            certificatePdf);

                        TempData["SuccessMessage"] = "Donation approved and certificate sent successfully.";
                    }
                }
                else
                {
                    _logger.LogError("Failed to retrieve donation details after approval. Status code: {StatusCode}", donationResponse.StatusCode);
                    TempData["ErrorMessage"] = "Donation approved but failed to send certificate.";
                }

                return RedirectToAction(nameof(DonationManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while approving donation ID: {DonationId}", id);
                TempData["ErrorMessage"] = "Failed to process donation approval.";
                return RedirectToAction(nameof(DonationManagement));
            }
        }

        // Get the details for a specific donation

        public async Task<IActionResult> DonationDetails(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/admin/donations/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var donation = JsonConvert.DeserializeObject<Donation>(content);
                    return View(donation);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                _logger.LogError("Failed to retrieve donation details. Status code: {StatusCode}", response.StatusCode);
                TempData["ErrorMessage"] = "Failed to retrieve donation details.";
                return RedirectToAction(nameof(DonationManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving donation details for ID: {DonationId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving donation details.";
                return RedirectToAction(nameof(DonationManagement));
            }
        }

        // Allow an admin to download a user's document
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                using var response = await _httpClient.GetAsync($"api/admin/donations/{id}/document");

                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    // Validate received bytes
                    if (bytes == null || bytes.Length == 0)
                    {
                        _logger.LogWarning("Received empty document from API for donation ID: {DonationId}", id);
                        return NotFound("Document is empty");
                    }

                    _logger.LogInformation("Received document with size: {Size} bytes for donation ID: {DonationId}",
                        bytes.Length, id);

                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
                    var contentDisposition = response.Content.Headers.ContentDisposition;
                    var fileName = contentDisposition?.FileName?.Trim('"') ?? "document.pdf";

                    // Set explicit headers
                    Response.Headers["Content-Length"] = bytes.Length.ToString();
                    Response.Headers["Content-Disposition"] = $"attachment; filename=\"{fileName}\"";
                    Response.Headers["Content-Type"] = contentType;

                    return new FileContentResult(bytes, contentType)
                    {
                        FileDownloadName = fileName
                    };
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Document not found");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to download document. Status code: {StatusCode}, Content: {Content}",
                    response.StatusCode, errorContent);
                return StatusCode(500, "Error downloading document");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while downloading document for donation ID: {DonationId}", id);
                return StatusCode(500, "Error downloading document");
            }
        }


        // Funding Management actions

        // Fetch all funding requests

        // GET: Admin/FundingRequestManagement
        public async Task<IActionResult> FundingRequestManagement()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Admin/FundingRequestManagement");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var fundingRequests = JsonConvert.DeserializeObject<List<FundingRequest>>(content);

                return View(fundingRequests);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching funding requests: {ex.Message}");
                return View("Error"); 
            }
        }

        // Allow an admin to view attachments
        public async Task<IActionResult> ViewAttachments(int requestId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Admin/FundingRequestAttachments/{requestId}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var attachments = JsonConvert.DeserializeObject<List<AttachmentViewModel>>(content);

                return View(attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching attachments: {ex.Message}");
                return View("Error");
            }
        }

        // Allow an admin to download an attachment
        public async Task<IActionResult> DownloadAttachment(int attachmentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Admin/DownloadAttachment/{attachmentId}");
                response.EnsureSuccessStatusCode();

                // Extract the file name from Content-Disposition, handling quotes if present
                var contentDisposition = response.Content.Headers.ContentDisposition;
                var fileName = contentDisposition?.FileName?.Trim('"') ?? "download";

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var fileBytes = await response.Content.ReadAsByteArrayAsync();

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading attachment: {ex.Message}");
                return View("Error");
            }
        }

        // Get details for a specific funding request 

        // GET: Admin/FundingRequestDetails/{id}
        public async Task<IActionResult> FundingRequestDetails(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Admin/FundingRequestDetails/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Funding request with ID {id} not found.");
                    return NotFound();
                }

                var content = await response.Content.ReadAsStringAsync();
                var fundingRequest = JsonConvert.DeserializeObject<FundingRequest>(content);

                return View(fundingRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching funding request details: {ex.Message}");
                return View("Error");
            }
        }

        // Allow the admin to approve a funding request

        // POST: Admin/ApproveFundingRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveFundingRequest(int id, string adminMessage)
        {
            try
            {
                var messageContent = JsonConvert.SerializeObject(adminMessage);
                var content = new StringContent(messageContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"api/Admin/ApproveFundingRequest?id={id}", content);
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(FundingRequestManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error approving funding request: {ex.Message}");
                return View("Error");
            }
        }

        // Allow the admin to reject a funding request

        // POST: Admin/RejectFundingRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectFundingRequest(int id)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/Admin/RejectFundingRequest?id={id}", null);
                response.EnsureSuccessStatusCode();

                return RedirectToAction(nameof(FundingRequestManagement));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error rejecting funding request: {ex.Message}");
                return View("Error");
            }
        }

        // Document Management actions

        // Get all documents in the database

        // GET: Admin/Documents
        public async Task<IActionResult> Documents()
        {
            List<Document> documents = new List<Document>();
            HttpResponseMessage response = await _httpClient.GetAsync("api/admin/documents");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                documents = JsonConvert.DeserializeObject<List<Document>>(json);
            }
            else
            {
                _logger.LogError("Failed to fetch documents. Status Code: {StatusCode}", response.StatusCode);
            }

            return View(documents);
        }

        // Allow an admin to verify a document

        // POST: Admin/ApproveDocument
        [HttpPost]
        public async Task<IActionResult> ApproveDocument(int documentId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(documentId), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"api/admin/approve-document?documentId={documentId}", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to approve document {DocumentId}. Status Code: {StatusCode}", documentId, response.StatusCode);
            }

            return RedirectToAction("Documents");
        }

        // Allow an admin to reject a document
        
        // POST: Admin/RejectDocument
        [HttpPost]
        public async Task<IActionResult> RejectDocument(int documentId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(documentId), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"api/admin/reject-document?documentId={documentId}", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to reject document {DocumentId}. Status Code: {StatusCode}", documentId, response.StatusCode);
            }

            return RedirectToAction("Documents");
        }

        // Allow an admin to mark a document as "received"

        // POST: Admin/DocumentsReceived
        [HttpPost]
        public async Task<IActionResult> DocumentsReceived(int documentId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(documentId), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"api/admin/documents-received?documentId={documentId}", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to mark document {DocumentId} as received. Status Code: {StatusCode}", documentId, response.StatusCode);
            }

            return RedirectToAction("Documents");
        }

        // Allow an admin to close a funding request

        // POST: Admin/CloseRequest
        [HttpPost]
        public async Task<IActionResult> CloseRequest(int documentId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(documentId), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync($"api/admin/close-request?documentId={documentId}", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to close request for document {DocumentId}. Status Code: {StatusCode}", documentId, response.StatusCode);
            }

            return RedirectToAction("Documents");
        }

        // Allow an admin to download a document

        // GET: Admin/DownloadDocument
        public async Task<IActionResult> DownloadDocument(int documentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/admin/download-document/{documentId}");
                response.EnsureSuccessStatusCode();  // Ensures the status code is successful

                // Extract the file name from Content-Disposition, handling quotes if present
                var contentDisposition = response.Content.Headers.ContentDisposition;
                var fileName = contentDisposition?.FileName?.Trim('"') ?? "download";

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var fileBytes = await response.Content.ReadAsByteArrayAsync();

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading document: {ex.Message}");
                return View("Error");  // Return an error view if something goes wrong
            }
        }

        // Report Creation actions

        // Get details to create a report about all the donations
        // GET: Admin/DonationReport
        public async Task<ActionResult> DonationReport()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/donation-report");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var donations = JsonConvert.DeserializeObject<List<DonationReportItem>>(content);
                    return View(donations);
                }
                else
                {
                    _logger.LogError("Failed to fetch donation report. Status code: {StatusCode}", response.StatusCode);
                    
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching donation report");
                return View("Error");
            }
        }

        // Get all the details to create a report about project funding requests
        // GET: Admin/FundingRequestReport
        public async Task<ActionResult> FundingRequestReport()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/funding-request-report");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var requests = JsonConvert.DeserializeObject<List<FundingRequestReportItem>>(content);
                    return View(requests);
                }
                else
                {
                    _logger.LogError("Failed to fetch funding request report. Status code: {StatusCode}", response.StatusCode);
                    
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching funding request report");
                return View("Error");
            }
        }
    }
}
