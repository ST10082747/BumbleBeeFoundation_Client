using Microsoft.AspNetCore.Mvc;
using BumbleBeeFoundation_Client.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace BumbleBeeFoundation_Client.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApiBaseUrl"]); 
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
            if (!ModelState.IsValid) return View(model);

            var response = await _httpClient.PostAsJsonAsync("api/account/login", model);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LoginResponse>(content); // Deserialize response

                // Simulate setting a session token or user data in cookies or session
                HttpContext.Session.SetString("UserId", result.UserId.ToString());
                HttpContext.Session.SetString("UserRole", result.Role);

                if (result.Role == "Company")
                {
                    HttpContext.Session.SetString("CompanyID", result.CompanyID.ToString());
                    HttpContext.Session.SetString("CompanyName", result.CompanyName);
                }

                // Redirect to home or another page after successful login
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt.");
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
    }
}
