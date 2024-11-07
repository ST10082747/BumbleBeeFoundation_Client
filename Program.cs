using BumbleBeeFoundation_Client.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    // Configure session timeout and other options
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make the session cookie HttpOnly
    options.Cookie.IsEssential = true; // Mark session cookie as essential
});

// Register HttpClient for API communication
builder.Services.AddHttpClient();  // Registers the default HttpClient
builder.Services.AddHttpClient("ApiHttpClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7181/");
    // Optionally, add any default headers, timeouts, etc.
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});


// Example: Bind SmtpSettings from appsettings.json (uncomment if you need it)
// builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Example: Register EmailService (uncomment if you need it)
// builder.Services.AddTransient<IEmailService, EmailService>();

// Example: Register other services like CertificateService (uncomment if needed)
// builder.Services.AddScoped<CertificateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Enable session middleware
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


