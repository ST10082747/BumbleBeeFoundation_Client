using BumbleBeeFoundation_Client.Models;
using BumbleBeeFoundation_Client.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache(); 
builder.Services.AddSession(options =>
{
    // Configure session timeout and other options
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

// Register HttpClient for API communication
builder.Services.AddHttpClient();  
builder.Services.AddHttpClient("ApiHttpClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7181/");
    // Add any default headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Register CurrencyConverterService with HttpClient
builder.Services.AddHttpClient<ICurrencyConverterService, CurrencyConverterService>();

// Bind SmtpSettings from appsettings.json
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

// Register EmailService
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddScoped<CertificateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); 
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


