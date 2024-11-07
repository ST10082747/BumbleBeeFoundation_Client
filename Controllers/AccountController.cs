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
                var result = JsonConvert.DeserializeObject<LoginResponse>(content);

                // Store user data in session
                HttpContext.Session.SetString("UserId", result.UserId.ToString());
                HttpContext.Session.SetString("UserRole", result.Role);
                HttpContext.Session.SetString("UserEmail", result.UserEmail);

                if (result.Role == "Company")
                {
                    // Store company ID as an integer in the session
                    HttpContext.Session.SetInt32("CompanyID", (int)result.CompanyID);

                    HttpContext.Session.SetString("CompanyName", result.CompanyName);
                    HttpContext.Session.SetString("UserId", result.UserId.ToString());

                    // Log after setting the session values
                    _logger.LogInformation("Session set: CompanyID - {CompanyID}, CompanyName - {CompanyName}, UserId - {UserId}",
                                           result.CompanyID, result.CompanyName, result.UserId);

                    return RedirectToAction("Index", "Company");
                }

                else if (result.Role == "Admin")
                {
                    // Redirect to Admin dashboard
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (result.Role == "Donor")
                {
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
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/account/register", model);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to login page after successful registration
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Registration failed. Please try again.");
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

            var response = await _httpClient.PostAsJsonAsync("api/account/forgotpassword", model);

            if (response.IsSuccessStatusCode)
            {
                // Show a message that reset instructions were sent
                ViewBag.Message = "Password reset instructions have been sent to your email.";
            }
            else
            {
                ModelState.AddModelError("", "Email not found.");
            }

            return View(model);
        }

        // GET: /Account/ResetPassword
        public IActionResult ResetPassword()
        {
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/account/resetpassword", model);

            if (response.IsSuccessStatusCode)
            {
                // Redirect to login after successful password reset
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", "Password reset failed. Please try again.");
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
