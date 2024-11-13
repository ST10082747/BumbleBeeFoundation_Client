using Microsoft.AspNetCore.Mvc;
using BumbleBeeFoundation_Client.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace BumbleBeeFoundation_Client.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AccountController> logger)
        {
            // Use the named client if you registered it as "ApiHttpClient"
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient"); // If you registered it with a name in Program.cs

            _configuration = configuration;
            _logger = logger;
        }


        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _httpClient.PostAsJsonAsync("api/account/login", model);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API Response Content: {Content}", content);

                var result = JsonConvert.DeserializeObject<LoginResponse>(content);
                _logger.LogInformation("Deserialized Result: {@Result}", result);

                // Check for null values before setting session
                if (result.UserId == null)
                {
                    _logger.LogError("UserId is null in login response");
                    ModelState.AddModelError(string.Empty, "Invalid login response from server");
                    return View(model);
                }

                if (string.IsNullOrEmpty(result.Role))
                {
                    _logger.LogError("Role is null in login response");
                    ModelState.AddModelError(string.Empty, "Invalid login response from server");
                    return View(model);
                }

                if (string.IsNullOrEmpty(result.UserEmail))
                {
                    _logger.LogError("UserEmail is null in login response");
                    ModelState.AddModelError(string.Empty, "Invalid login response from server");
                    return View(model);
                }

                if (string.IsNullOrEmpty(result.FirstName))
                {
                    _logger.LogError("User Name is null in login response");
                    ModelState.AddModelError(string.Empty, "Invalid login response from server");
                    return View(model);
                }

                if (string.IsNullOrEmpty(result.LastName))
                {
                    _logger.LogError("Last Name is null in login response");
                    ModelState.AddModelError(string.Empty, "Invalid login response from server");
                    return View(model);
                }

                // Now set the session values
                HttpContext.Session.SetString("UserId", result.UserId.ToString());
                HttpContext.Session.SetString("UserRole", result.Role);
                HttpContext.Session.SetString("UserEmail", result.UserEmail);
                HttpContext.Session.SetString("FirstName", result.FirstName);
                HttpContext.Session.SetString("LastName", result.LastName);
               

                if (result.Role == "Company")
                {
                    // Store company ID as an integer in the session
                    HttpContext.Session.SetInt32("CompanyID", (int)result.CompanyID);

                    HttpContext.Session.SetString("CompanyName", result.CompanyName);
                    HttpContext.Session.SetString("UserId", result.UserId.ToString());
                    HttpContext.Session.SetString("FirstName", result.FirstName);
                    HttpContext.Session.SetString("LastName", result.LastName);

                    // Log after setting the session values
                    _logger.LogInformation("Session set: CompanyID - {CompanyID}, CompanyName - {CompanyName}, UserId - {UserId}",
                                           result.CompanyID, result.CompanyName, result.UserId);

                    return RedirectToAction("Index", "Company");
                }

                else if (result.Role == "Admin")
                {
                    HttpContext.Session.SetString("FirstName", result.FirstName);
                    HttpContext.Session.SetString("LastName", result.LastName);
                    // Redirect to Admin dashboard
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (result.Role == "Donor")
                {
                    // Store user data in session
                    HttpContext.Session.SetString("UserId", result.UserId.ToString());
                    HttpContext.Session.SetString("UserRole", result.Role);
                    HttpContext.Session.SetString("UserEmail", result.UserEmail);
                    HttpContext.Session.SetString("FirstName", result.FirstName);
                    HttpContext.Session.SetString("LastName", result.LastName);
                    // Redirect to Donor index
                    return RedirectToAction("Index", "Donor");
                }
                else
                {
                    // Redirect to Home page
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Log the validation errors for ModelState
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError($"ModelState error for {state.Key}: {error.ErrorMessage}");
                    }
                }

                ModelState.AddModelError("", "Registration failed due to validation errors. Please check your input.");
                return View(model);
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/account/register", model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Registration failed: " + errorResponse);
                    _logger.LogError($"Registration failed. Response from API: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while attempting to register: {ex.Message}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return View(model);
        }


        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/account/forgot-password", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", "Forgot Password failed: " + errorResponse);
            return View(model);
        }

        // GET: /Account/ResetPassword
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/account/reset-password", model);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            var errorResponse = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", "Password reset failed: " + errorResponse);
            return View(model);
        }


        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
