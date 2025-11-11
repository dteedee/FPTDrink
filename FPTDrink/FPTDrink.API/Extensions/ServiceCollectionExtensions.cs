using FPTDrink.Core.Interfaces;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Infrastructure.Data;
using FPTDrink.Infrastructure.Repositories;
using FPTDrink.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.API.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			// Đăng ký DbContext (đọc chuỗi kết nối từ appsettings: ConnectionStrings:DefaultConnection)
			var connectionString = configuration.GetConnectionString("DefaultConnection");
			if (!string.IsNullOrWhiteSpace(connectionString))
			{
				services.AddDbContext<FptdrinkContext>(options =>
					options.UseSqlServer(connectionString));
			}

			// Đăng ký repository/unit of work
			services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
			services.AddScoped<ICategoryRepository, CategoryRepository>();
			services.AddScoped<IChucVuRepository, ChucVuRepository>();
			services.AddScoped<IPhanQuyenRepository, PhanQuyenRepository>();
			services.AddScoped<INhanVienRepository, NhanVienRepository>();
			services.AddScoped<IHoaDonRepository, HoaDonRepository>();
			services.AddScoped<IChiTietHoaDonRepository, ChiTietHoaDonRepository>();
			services.AddScoped<IProductRepository, ProductRepository>();
			services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<ICategoryService, CategoryService>();
			services.AddScoped<IChucVuService, ChucVuService>();
			services.AddScoped<IReportingService, ReportingService>();
			services.AddScoped<IAuthService, AuthService>();
			services.AddScoped<IOrderService, OrderService>();
			services.AddScoped<IProductCategoryService, ProductCategoryService>();
			services.AddScoped<INhanVienService, NhanVienService>();

			return services;
		}
	}
}


