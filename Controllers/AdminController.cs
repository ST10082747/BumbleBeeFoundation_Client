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

        // Dashboard Action - Fetches statistics from the API
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/admin/dashboard");
                response.EnsureSuccessStatusCode(); // Throws an exception if the status code is not successful

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

        // User Management - Fetches users from the API
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

        // Create User - Displays the user creation form
        public IActionResult CreateUser()
        {
            return View();
        }

        // POST: Create User - Sends data to the API to create a user
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

        // Edit User - Fetches the user to edit from the API
        public async Task<IActionResult> EditUser(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/admin/users/{id}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(json);

                // Map User to UserForEdit
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


        // POST: Edit User - Sends the updated user data to the API
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

        // Delete User - Fetches the user to confirm deletion
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

        // POST: Delete User - Sends a delete request to the API
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


        // company management
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



        // donations
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

        [HttpPost]
        public async Task<IActionResult> ApproveDonation(int id)
        {
            try
            {
                // First, approve the donation through the API
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

                        // Send email with certificate
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



    }
}
