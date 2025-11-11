var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Bind configuration options (Email, VNPay) nếu dùng trong Web layer
builder.Services.Configure<FPTDrink.Web.Extensions.EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<FPTDrink.Web.Extensions.VnPayOptions>(builder.Configuration.GetSection("VNPay"));

// Register DbContext & visitor stats services for direct service calls
builder.Services.AddDbContext<FPTDrink.Infrastructure.Data.FptdrinkContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
builder.Services.AddSingleton<FPTDrink.Core.Interfaces.Services.IVisitorsOnlineTracker, FPTDrink.Infrastructure.Services.VisitorsOnlineTracker>();
builder.Services.AddScoped<FPTDrink.Core.Interfaces.Services.IVisitorStatsService, FPTDrink.Infrastructure.Services.VisitorStatsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/Error");
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Track visitors for online count
app.UseVisitorsTracking();

// Legacy routes ported from MVC 5
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
	pattern: "danh-muc-san-pham/{alias}-{id?}",
	defaults: new { controller = "Products", action = "ProductCategory" });

app.MapControllerRoute(
	name: "TraCuuDonHang",
	pattern: "tra-cuu-don-hang",
	defaults: new { controller = "TraCuuDonHang", action = "Index" });

app.MapControllerRoute(
	name: "SupplierProduct",
	pattern: "nha-cung-cap/{alias}-{id?}",
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
