using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using FPTDrink.Infrastructure.Extensions;
using FPTDrink.Web.Extensions;
using System.Net.Http;
using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using System.Collections.Generic;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<FPTDrink.Web.Extensions.EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<FPTDrink.Web.Extensions.VnPayOptions>(builder.Configuration.GetSection("VNPay"));
builder.Services.Configure<FPTDrink.Core.Interfaces.Options.JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddDbContext<FPTDrink.Infrastructure.Data.FptdrinkContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.Cookie.Name = ".FPTDrink.Session";
	options.IdleTimeout = TimeSpan.FromMinutes(60);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
	// Trong development, dùng Lax để tránh lỗi OAuth state
	// Trong production, có thể cần None nếu dùng cross-domain
	options.Cookie.SameSite = SameSiteMode.Lax;
	options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
		? CookieSecurePolicy.None 
		: CookieSecurePolicy.SameAsRequest;
	options.Cookie.Path = "/";
});

// Data Protection - Cần thiết cho OAuth state
var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
if (!Directory.Exists(dataProtectionKeysPath))
{
	Directory.CreateDirectory(dataProtectionKeysPath);
}
var dataProtectionBuilder = builder.Services.AddDataProtection()
	.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
	.SetApplicationName("FPTDrink");

// Trong development, không cần encrypt quá mạnh
if (builder.Environment.IsDevelopment())
{
	dataProtectionBuilder.SetDefaultKeyLifetime(TimeSpan.FromDays(90));
}
builder.Services.AddFptDrinkInfrastructure(builder.Configuration);

var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl");
builder.Services.AddHttpClient("FPTDrinkApi", client =>
{
	var baseAddress = string.IsNullOrWhiteSpace(apiBaseUrl) ? "https://localhost:5001" : apiBaseUrl!;
	client.BaseAddress = new Uri(baseAddress);
	client.DefaultRequestHeaders.Accept.Clear();
	client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
	client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
	ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});
builder.Services.AddScoped<FPTDrink.Web.Services.ApiClient>();
builder.Services.AddHttpContextAccessor();

// Authentication configuration
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
	options.Cookie.Name = ".FPTDrink.Auth";
	options.Cookie.SameSite = SameSiteMode.Lax;
	options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
		? CookieSecurePolicy.None 
		: CookieSecurePolicy.SameAsRequest;
	options.Cookie.HttpOnly = true;
	options.Cookie.Path = "/";
	options.ExpireTimeSpan = TimeSpan.FromDays(30);
	options.SlidingExpiration = true;
	// Đảm bảo cookie được gửi trong mọi request
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/Error");

// Configure static files - serve from wwwroot
app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
    }
});

// Serve static files from Uploads folder (outside wwwroot) - for backward compatibility
// This allows serving files from FPTDrink.Web\Uploads\files if they exist there
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads", "files");
if (Directory.Exists(uploadsPath))
{
    app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsPath),
        RequestPath = "/Uploads/files",
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        }
    });
}


// Session phải được load TRƯỚC Authentication để OAuth state có thể được lưu
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseVisitorsTracking();

app.MapAreaControllerRoute(
	name: "admin",
	areaName: "Admin",
	pattern: "admin/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
	name: "Contact",
	pattern: "lien-he",
	defaults: new { controller = "Contact", action = "Index" });

app.MapControllerRoute(
	name: "CheckOut",
	pattern: "lap-hoa-don",
	defaults: new { controller = "ShoppingCart", action = "CheckOut" });

app.MapControllerRoute(
	name: "vnpay_return",
	pattern: "vnpay_return",
	defaults: new { controller = "ShoppingCart", action = "VnpayReturn" });

app.MapControllerRoute(
	name: "ShoppingCart",
	pattern: "gio-hang",
	defaults: new { controller = "ShoppingCart", action = "Index" });

app.MapControllerRoute(
	name: "CategoryProduct",
	pattern: "danh-muc-san-pham/{alias}-{id}",
	defaults: new { controller = "Products", action = "ProductCategory" });

app.MapControllerRoute(
	name: "TraCuuDonHang",
	pattern: "tra-cuu-don-hang",
	defaults: new { controller = "TraCuuDonHang", action = "Index" });

app.MapControllerRoute(
	name: "SupplierProduct",
	pattern: "nha-cung-cap/{alias}-{id}",
	defaults: new { controller = "Products", action = "Supplier" });

app.MapControllerRoute(
	name: "BaiViet",
	pattern: "post/{alias?}",
	defaults: new { controller = "Article", action = "Index" });

app.MapControllerRoute(
	name: "detailProduct",
	pattern: "chi-tiet/{alias}-p{id}",
	defaults: new { controller = "Products", action = "Detail" });

app.MapControllerRoute(
	name: "Products",
	pattern: "san-pham",
	defaults: new { controller = "Products", action = "Index" });

app.MapControllerRoute(
	name: "DetailNew",
	pattern: "{alias}-n{id}",
	defaults: new { controller = "News", action = "Detail" });

app.MapControllerRoute(
	name: "NewsList",
	pattern: "tin-tuc",
	defaults: new { controller = "News", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
