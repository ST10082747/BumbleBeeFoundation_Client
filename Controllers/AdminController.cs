using BumbleBeeFoundation_Client.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BumbleBeeFoundation_Client.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly HttpClient _httpClient;

        public AdminController(
            ILogger<AdminController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("ApiHttpClient");
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


    }
}
