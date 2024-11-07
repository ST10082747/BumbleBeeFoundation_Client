using Microsoft.AspNetCore.Mvc;
using System.Text;
using BumbleBeeFoundation_Client.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
//using System.Text.Json;
//using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BumbleBeeFoundation_Client.Controllers
{
    public class CompanyController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CompanyController> _logger;
        private readonly IConfiguration _configuration;

        public CompanyController(IHttpClientFactory httpClientFactory, ILogger<CompanyController> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");  // Use the named client
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var companyId = HttpContext.Session.GetInt32("CompanyID");
            var userId = HttpContext.Session.GetString("UserId");

            // Ensure both CompanyID and UserId are available in session
            if (companyId == null || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Session data missing, redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            var response = await _httpClient.GetAsync($"api/Company/{companyId}?userId={userId}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var content = await response.Content.ReadAsStringAsync();
            try
            {
                // Using Newtonsoft.Json for deserialization
                var companyInfo = JsonConvert.DeserializeObject<CompanyViewModel>(content);
                _logger.LogInformation("CompanyName: {0}, ContactEmail: {1}", companyInfo?.CompanyName, companyInfo?.ContactEmail);

                if (companyInfo == null)
                {
                    _logger.LogWarning("Deserialization returned null for CompanyViewModel.");
                    return View("Error"); // Or handle appropriately
                }

                return View(companyInfo);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize CompanyViewModel from API response.");
                return View("Error");
            }
        }


        public IActionResult RequestFunding()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RequestFunding(FundingRequestViewModel model, List<IFormFile> attachments)
        {
            _logger.LogInformation("ModelState is valid: {IsValid}", ModelState.IsValid);
            foreach (var key in ModelState.Keys)
            {
                if (ModelState[key].Errors.Count > 0)
                {
                    foreach (var error in ModelState[key].Errors)
                    {
                        _logger.LogWarning("ModelState error: {Key} - {ErrorMessage}", key, error.ErrorMessage);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var companyId = HttpContext.Session.GetInt32("CompanyID");
            if (companyId == null)
            {
                _logger.LogError("CompanyID not found in session.");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("CompanyID retrieved from session: {CompanyID}", companyId.Value);
            model.CompanyID = companyId.Value;

            using var content = new MultipartFormDataContent();

            // Add individual fields to content
            content.Add(new StringContent(model.CompanyID.ToString()), "CompanyID");
            content.Add(new StringContent(model.ProjectDescription ?? ""), "ProjectDescription");
            content.Add(new StringContent(model.RequestedAmount.ToString()), "RequestedAmount");
            content.Add(new StringContent(model.ProjectImpact ?? ""), "ProjectImpact");
            content.Add(new StringContent(model.Status ?? ""), "Status");
            content.Add(new StringContent(model.SubmittedAt.ToString("o")), "SubmittedAt"); // Consistent date format

            // Add each attachment as a StreamContent
            if (attachments != null && attachments.Count > 0)
            {
                foreach (var file in attachments)
                {
                    if (file.Length > 0)
                    {
                        var fileContent = new StreamContent(file.OpenReadStream());
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        content.Add(fileContent, "attachments", file.FileName);
                    }
                }
            }

            try
            {
                _logger.LogInformation("Sending funding request to API endpoint: {Url}", "api/Company/RequestFunding");
                var response = await _httpClient.PostAsync("api/Company/RequestFunding", content);

                _logger.LogInformation("API response status code: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to submit funding request. Status Code: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);

                    ModelState.AddModelError("", "Failed to submit the funding request. Please try again.");
                    return View(model);
                }

                var requestId = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Funding request submitted successfully. RequestID: {RequestID}", requestId);

                return RedirectToAction(nameof(FundingRequestConfirmation), new { id = requestId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting funding request.");
                ModelState.AddModelError("", "An error occurred while submitting the funding request.");
                return View(model);
            }
        }



        public async Task<IActionResult> FundingRequestConfirmation(int id)
        {
            var response = await _httpClient.GetAsync($"api/Company/FundingRequestConfirmation/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var content = await response.Content.ReadAsStringAsync();

            // Use Newtonsoft.Json for deserialization
            var fundingRequest = JsonConvert.DeserializeObject<FundingRequestViewModel>(content);

            if (fundingRequest == null)
            {
                return View("Error"); // Handle deserialization failure appropriately
            }

            return View(fundingRequest);
        }

        public async Task<IActionResult> FundingRequestHistory()
        {
            var companyId = HttpContext.Session.GetInt32("CompanyID");
            if (companyId == null)
            {
                _logger.LogWarning("Session does not contain CompanyID, redirecting to login.");
                return RedirectToAction("Login", "Account");
            }

            var response = await _httpClient.GetAsync($"api/Company/FundingRequestHistory/{companyId}");
            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var content = await response.Content.ReadAsStringAsync();
            var requests = JsonConvert.DeserializeObject<List<FundingRequestViewModel>>(content);

            if (requests == null)
            {
                return View("Error");
            }

            return View(requests);
        }

        public async Task<IActionResult> DownloadAttachment(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Company/DownloadAttachment/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to download attachment with ID {Id}. Status code: {StatusCode}",
                        id, response.StatusCode);
                    return NotFound();
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                var contentDisposition = response.Content.Headers.ContentDisposition;

                // Get filename from Content-Disposition header
                string fileName = null;
                if (contentDisposition != null)
                {
                    fileName = contentDisposition.FileName?.Trim('"');  // Remove quotes if present

                    // If filename is still null, try FileNameStar
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = contentDisposition.FileNameStar?.Trim('"');
                    }

                    // If still null, extract from the raw header value
                    if (string.IsNullOrEmpty(fileName))
                    {
                        var rawHeader = response.Content.Headers.GetValues("Content-Disposition").FirstOrDefault();
                        if (rawHeader != null)
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(rawHeader, @"filename=""?([^""]+)""?");
                            if (match.Success)
                            {
                                fileName = match.Groups[1].Value;
                            }
                        }
                    }
                }

                // Fallback filename if none found
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"attachment_{id}{GetFileExtensionFromContentType(response.Content.Headers.ContentType?.MediaType)}";
                }

                // Clean the filename
                fileName = System.IO.Path.GetFileName(fileName);  // Remove any path components
                fileName = Uri.UnescapeDataString(fileName);     // Decode URL encoding

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                _logger.LogInformation("Downloading file: {FileName}, Type: {ContentType}, Size: {Size} bytes",
                    fileName, contentType, content.Length);

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment with ID {Id}", id);
                return StatusCode(500, "An error occurred while downloading the attachment.");
            }
        }

        private string GetFileExtensionFromContentType(string contentType)
        {
            return contentType?.ToLower() switch
            {
                "application/pdf" => ".pdf",
                "application/msword" => ".doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "text/plain" => ".txt",
                _ => ""
            };
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int requestId, IFormFile document)
        {
            if (document == null || document.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var companyId = HttpContext.Session.GetInt32("CompanyID");
            if (companyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(requestId.ToString()), "requestId");

            var fileContent = new StreamContent(document.OpenReadStream())
            {
                Headers =
            {
                ContentType = new MediaTypeHeaderValue(document.ContentType)
            }
            };
            content.Add(fileContent, "document", document.FileName);

            var response = await _httpClient.PostAsync("api/Company/UploadDocument", content);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to upload the document. Please try again.");
            }

            return RedirectToAction("FundingRequestHistory");
        }
    }
}
