using FPTDrink.Core.Interfaces;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Infrastructure.Data;
using FPTDrink.Infrastructure.Repositories;
using FPTDrink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FPTDrink.Web.Extensions
{
	public static class ServiceRegistrationExtensions
	{
		public static IServiceCollection AddFptDrinkInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("MyCnn");
			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				services.AddDbContext<FptdrinkContext>(options =>
					options.UseSqlServer(connectionString));
			}

			services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
			services.AddScoped<ICategoryRepository, CategoryRepository>();
			services.AddScoped<IChucVuRepository, ChucVuRepository>();
			services.AddScoped<IPhanQuyenRepository, PhanQuyenRepository>();
			services.AddScoped<INhanVienRepository, NhanVienRepository>();
			services.AddScoped<IHoaDonRepository, HoaDonRepository>();
			services.AddScoped<IChiTietHoaDonRepository, ChiTietHoaDonRepository>();
			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
			services.AddScoped<INhaCungCapRepository, NhaCungCapRepository>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddScoped<ICategoryService, CategoryService>();
			services.AddScoped<IChucVuService, ChucVuService>();
			services.AddScoped<IReportingService, ReportingService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IOrderService, OrderService>();
			services.AddScoped<IProductCategoryService, ProductCategoryService>();
			services.AddScoped<INhanVienService, NhanVienService>();
			services.AddScoped<IProductService, ProductService>();
			services.AddScoped<INhaCungCapService, NhaCungCapService>();
			services.AddScoped<ICatalogQueryService, CatalogQueryService>();
			services.AddScoped<IHomePageService, HomePageService>();
			services.AddScoped<IProductPublicService, ProductPublicService>();
			services.AddScoped<IMenuQueryService, MenuQueryService>();
			services.AddScoped<ISearchService, SearchService>();
			services.AddScoped<IOrderPublicQueryService, OrderPublicQueryService>();
			services.AddSingleton<ICartStore, InMemoryCartStore>();
			services.AddScoped<ICartService, CartService>();
			services.AddScoped<ICheckoutService, CheckoutService>();
			services.AddScoped<IPaymentService, VnPayService>();
			services.AddScoped<IEmailService, SmtpEmailService>();
			services.AddSingleton<IVisitorsOnlineTracker, VisitorsOnlineTracker>();
			services.AddScoped<IVisitorStatsService, VisitorStatsService>();

			return services;
		}
	}
}

