using FPTDrink.Core.Interfaces;
using FPTDrink.Infrastructure.Data;
using FPTDrink.Infrastructure.Repositories;
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
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			return services;
		}
	}
}


