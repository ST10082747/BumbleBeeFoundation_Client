using BumbleBeeFoundation_Client.Models;
using BumbleBeeFoundation_Client.Services;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Web;

namespace BumbleBeeFoundation_Client.Controllers
{
    public class DonorController : Controller
    {
        private readonly ILogger<DonorController> _logger;
        private readonly IEmailService _emailService;
        private readonly PayFastSettings _payFastSettings;
        private readonly HttpClient _httpClient;

        public DonorController(
            IConfiguration configuration,
            ILogger<DonorController> logger,
            IEmailService emailService,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _emailService = emailService;
            _payFastSettings = configuration.GetSection("PayFast").Get<PayFastSettings>();
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Donor/FundingRequests");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var fundingRequests = JsonConvert.DeserializeObject<List<FundingRequestViewModel>>(content);
                    return View(fundingRequests);
                }

                _logger.LogError($"API call failed with status code: {response.StatusCode}");
                return View(new List<FundingRequestViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching funding requests");
                return View(new List<FundingRequestViewModel>());
            }
        }

        public IActionResult Donate()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Donate(DonationViewModel model, IFormFile? documentUpload)
        {
            if (!ModelState.IsValid || !IsValidIDNumber(model.DonorIDNumber))
            {
                ModelState.AddModelError("DonorIDNumber", "Invalid ID number.");
                return View(model);
            }

            if (documentUpload != null && documentUpload.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await documentUpload.CopyToAsync(memoryStream);
                    model.DocumentPath = memoryStream.ToArray();
                }
            }

            // Serialize the model using Newtonsoft.Json
            var jsonContent = JsonConvert.SerializeObject(model);
            var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/Donor/Donate", stringContent);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Failed to process donation. Please try again.");
                return View(model);
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DonationViewModel>(content);
            model.DonationId = result.DonationId;

            try
            {
                await _emailService.SendDonationNotificationAsync(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send donation notification email.");
            }

            var payFastRequest = new PayFastRequest
            {
                merchant_id = _payFastSettings.MerchantId,
                merchant_key = _payFastSettings.MerchantKey,
                name_first = model.DonorName?.Trim(),
                email_address = model.DonorEmail?.Trim(),
                m_payment_id = model.DonationId.ToString(),
                amount = model.DonationAmount.ToString("F2", CultureInfo.InvariantCulture),
                item_name = $"Funding Donation - {model.DonationType}".Trim(),
                payment_method = model.DonationType == "Monthly" ? "eft" : ""
            };

            string signature = payFastRequest.GenerateSignature(_payFastSettings.PassPhrase);

            var form = new StringBuilder();
            form.Append("<form action='");
            form.Append(_payFastSettings.UseSandbox ?
                "https://sandbox.payfast.co.za/eng/process" :
                "https://www.payfast.co.za/eng/process");
            form.Append("' method='post' id='PayFastForm'>");

            foreach (var prop in payFastRequest.GetType().GetProperties())
            {
                var value = prop.GetValue(payFastRequest);
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    form.Append($"<input type='hidden' name='{prop.Name}' value='{HttpUtility.HtmlEncode(value.ToString().Trim())}' />");
                }
            }

            form.Append($"<input type='hidden' name='signature' value='{HttpUtility.HtmlEncode(signature)}' />");
            form.Append("</form>");
            form.Append("<script>document.getElementById('PayFastForm').submit();</script>");

            return Content(form.ToString(), "text/html");
        }

        public async Task<IActionResult> DonationConfirmation(int id)
        {
            var response = await _httpClient.GetAsync($"api/Donor/Donation/{id}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var donation = JsonConvert.DeserializeObject<DonationViewModel>(content);
                return View(donation);
            }

            return NotFound();
        }

        public async Task<IActionResult> DonationHistory()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var response = await _httpClient.GetAsync($"api/Donor/Donations/User/{HttpUtility.UrlEncode(userEmail)}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var donations = JsonConvert.DeserializeObject<List<DonationViewModel>>(content);
                return View(donations);
            }

            return View(new List<DonationViewModel>());
        }

        public async Task<IActionResult> DownloadDonationHistory()
        {
            string userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var response = await _httpClient.GetAsync($"api/Donor/Donations/User/{HttpUtility.UrlEncode(userEmail)}");
            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(DonationHistory));
            }

            var content = await response.Content.ReadAsStringAsync();
            var donations = JsonConvert.DeserializeObject<List<DonationViewModel>>(content);

            using (var memoryStream = new MemoryStream())
            {
                var writerProperties = new WriterProperties();
                using (var pdfWriter = new PdfWriter(memoryStream, writerProperties))
                {
                    using (var pdfDoc = new PdfDocument(pdfWriter))
                    {
                        using (var document = new iText.Layout.Document(pdfDoc))
                        {
                            document.Add(new Paragraph("Bumble Bee Foundation")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(24)
                                .SetBold());

                            document.Add(new Paragraph($"Report For {userEmail}:")
                                .SetTextAlignment(TextAlignment.LEFT)
                                .SetFontSize(12)
                                .SetItalic());

                            document.Add(new Paragraph("Your Donation History for project funds")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(20)
                                .SetBold()
                                .SetMarginBottom(20));

                            Table table = new Table(5, false);

                            table.AddHeaderCell("Donation ID");
                            table.AddHeaderCell("Date");
                            table.AddHeaderCell("Type");
                            table.AddHeaderCell("Amount");
                            table.AddHeaderCell("Donor Name");

                            foreach (var donation in donations)
                            {
                                table.AddCell(donation.DonationId.ToString());
                                table.AddCell(donation.DonationDate.ToString("yyyy-MM-dd"));
                                table.AddCell(donation.DonationType);
                                table.AddCell(donation.DonationAmount.ToString("C"));
                                table.AddCell(donation.DonorName);
                            }

                            document.Add(table);

                            document.Add(new Paragraph("Thank you for donating to our foundation! Your contributions help us make a difference.")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(16)
                                .SetItalic()
                                .SetMarginTop(20));
                        }
                    }
                }

                return File(memoryStream.ToArray(), "application/pdf", "DonationHistory.pdf");
            }
        }

        public async Task<IActionResult> FundingRequests()
        {
            var response = await _httpClient.GetAsync("api/Donor/FundingRequests");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fundingRequests = JsonConvert.DeserializeObject<List<FundingRequestViewModel>>(content);
                return View(fundingRequests);
            }

            return View(new List<FundingRequestViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> SearchFundingRequests(string term)
        {
            var response = await _httpClient.GetAsync($"api/Donor/SearchFundingRequests?term={HttpUtility.UrlEncode(term)}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var fundingRequests = JsonConvert.DeserializeObject<List<FundingRequestViewModel>>(content);
                return Json(fundingRequests);
            }

            return Json(new List<FundingRequestViewModel>());
        }

        private bool IsValidIDNumber(string idNumber)
        {
            return idNumber.Length == 13 && idNumber.All(char.IsDigit);
        }
    }


}
