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

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/account/login", model);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response Content: {Content}", content);

                    var result = JsonConvert.DeserializeObject<LoginResponse>(content);
                    _logger.LogInformation("Deserialized Result: {@Result}", result);

                    // Check for null values before setting session
                    if (result.UserId == null || string.IsNullOrEmpty(result.Role) || string.IsNullOrEmpty(result.UserEmail) ||
                        string.IsNullOrEmpty(result.FirstName) || string.IsNullOrEmpty(result.LastName))
                    {
                        _logger.LogError("Invalid login response data");
                        ModelState.AddModelError(string.Empty, "Invalid login response from server");
                        return View(model);
                    }

                    // Set session variables
                    HttpContext.Session.SetString("UserId", result.UserId.ToString());
                    HttpContext.Session.SetString("UserRole", result.Role);
                    HttpContext.Session.SetString("UserEmail", result.UserEmail);
                    HttpContext.Session.SetString("FirstName", result.FirstName);
                    HttpContext.Session.SetString("LastName", result.LastName);

                    if (result.Role == "Company")
                    {
                        HttpContext.Session.SetInt32("CompanyID", (int)result.CompanyID);
                        HttpContext.Session.SetString("CompanyName", result.CompanyName);
                        return RedirectToAction("Index", "Company");
                    }
                    else if (result.Role == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    else if (result.Role == "Donor")
                    {
                        return RedirectToAction("Index", "Donor");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    _logger.LogError("API login request failed: {StatusCode}", response.StatusCode);
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // Handle API downtime or network errors
                _logger.LogError("Error during API login request: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, "Login Denied - the server is currently offline. Please try again later.");
                return View(model);
            }
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
                // Log the exception and notify the user
                _logger.LogError($"An error occurred while attempting to register: {ex.Message}");
                ModelState.AddModelError("", "Registration Failed - our server is currently offline. Please try again later.");
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
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/account/forgot-password", model);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("ResetPassword", new { email = model.Email });
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "Forgot Password failed: " + errorResponse);
                    _logger.LogError($"ForgotPassword API call failed: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while attempting to process forgot password: {ex.Message}");
                ModelState.AddModelError("", "Our server is currently offline. Please try again later.");
            }

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
