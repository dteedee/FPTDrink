using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using FPTDrink.Infrastructure.Extensions;
using System.Net.Http;
using System;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.Configure<FPTDrink.Web.Extensions.EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<FPTDrink.Web.Extensions.VnPayOptions>(builder.Configuration.GetSection("VNPay"));
builder.Services.AddDbContext<FPTDrink.Infrastructure.Data.FptdrinkContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
builder.Services.AddSingleton<FPTDrink.Core.Interfaces.Services.IVisitorsOnlineTracker, FPTDrink.Infrastructure.Services.VisitorsOnlineTracker>();
builder.Services.AddScoped<FPTDrink.Core.Interfaces.Services.IVisitorStatsService, FPTDrink.Infrastructure.Services.VisitorStatsService>();

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/Error");
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseVisitorsTracking();
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
