using Microsoft.AspNetCore.Mvc;
using System.Text;
using BumbleBeeFoundation_Client.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using BumbleBeeFoundation_Client.Services;

namespace BumbleBeeFoundation_Client.Controllers
{
    public class CompanyController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CompanyController> _logger;
        private readonly IConfiguration _configuration;

        private readonly ICurrencyConverterService _currencyConverterService;

        public CompanyController(IHttpClientFactory httpClientFactory, ILogger<CompanyController> logger, IConfiguration configuration, ICurrencyConverterService currencyConverterService)
        {
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");  
            _logger = logger;
            _configuration = configuration;
            _currencyConverterService = currencyConverterService;
        }

        // Fetch dashboard view data for companies
        public async Task<IActionResult> Index()
        {
            var companyId = HttpContext.Session.GetInt32("CompanyID");
            var userId = HttpContext.Session.GetString("UserId");

            // Check if both CompanyID and UserId are available in session
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
                    return View("Error"); 
                }

                return View(companyInfo);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize CompanyViewModel from API response.");
                return View("Error");
            }
        }

        // Allow users to perform currency conversions
        [HttpGet]
        public async Task<IActionResult> ConvertCurrency(decimal amount, string currencyCode)
        {
            _logger.LogInformation("ConvertCurrency action called with amount: {Amount} and currencyCode: {CurrencyCode}", amount, currencyCode);

            try
            {
                var convertedAmount = await _currencyConverterService.ConvertToRand(amount, currencyCode);
                _logger.LogInformation("Currency converted successfully. Amount in ZAR: {ConvertedAmount}", convertedAmount);

                return Json(new { success = true, convertedAmount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while converting currency for amount: {Amount} and currencyCode: {CurrencyCode}", amount, currencyCode);
                return Json(new { success = false, message = "Error occurred while converting currency." });
            }
        }

        public IActionResult RequestFunding()
        {
            return View();
        }

        // Process a company's request for funding

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

        // Once a donation has been saved to the database, provide the user with confirmation
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
                return View("Error"); 
            }

            return View(fundingRequest);
        }

        // Fetch the funding history for the company
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

        // Allow company employee to download files
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
                    fileName = contentDisposition.FileName?.Trim('"'); 

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
                fileName = System.IO.Path.GetFileName(fileName);  
                fileName = Uri.UnescapeDataString(fileName);    

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

        // Helper method to check files in database
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

        // Let a company upload documents

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int requestId, IFormFile document)
        {
            if (document == null || document.Length == 0)
            {
                ModelState.AddModelError("", "No file uploaded.");
                return RedirectToAction("FundingRequestHistory");
            }

            var companyId = HttpContext.Session.GetInt32("CompanyID");
            if (companyId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(requestId.ToString()), "requestId");
            content.Add(new StringContent(companyId.ToString()), "companyId"); 

            var fileContent = new StreamContent(document.OpenReadStream())
            {
                Headers = { ContentType = new MediaTypeHeaderValue(document.ContentType) }
            };
            content.Add(fileContent, "document", document.FileName);

            var response = await _httpClient.PostAsync("api/company/upload-document", content);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to upload the document. Please try again.");
            }

            return RedirectToAction("FundingRequestHistory");
        }


    }
}
