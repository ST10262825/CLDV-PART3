using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using JamesCrafts.Models;
using JamesCrafts;

var builder = WebApplication.CreateBuilder(args);

// Configuration for the connection string
var connectionString = builder.Configuration.GetConnectionString("MyConnectionStringDev")
  ?? throw new InvalidOperationException("Connection string 'MyConnectionStringDev' not found.");
//var connectionString = builder.Configuration.GetConnectionString("MyConnectionStringAzure")
   // ?? throw new InvalidOperationException("Connection string 'MyConnectionStringAzure' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext with SQL Server
builder.Services.AddDbContext<JamesContext>(options =>
    options.UseSqlServer(connectionString)
    .EnableSensitiveDataLogging(true)
    .LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<JamesContext>();

// Add role-based authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
});

// Add Razor Pages services
builder.Services.AddRazorPages();

// Add session services
builder.Services.AddSession(options =>
{
    // Configure session options
    options.Cookie.Name = ".JamesCrafts.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust session timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Map Razor Pages endpoints
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
