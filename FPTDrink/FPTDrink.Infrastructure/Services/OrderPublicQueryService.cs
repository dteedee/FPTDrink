using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class OrderPublicQueryService : IOrderPublicQueryService
	{
		private readonly IHoaDonRepository _orderRepo;

		public OrderPublicQueryService(IHoaDonRepository orderRepo)
		{
			_orderRepo = orderRepo;
		}

		public Task<HoaDon?> GetByCodeAsync(string maHoaDon, CancellationToken cancellationToken = default)
		{
			return _orderRepo.Query()
				.Where(o => o.MaHoaDon == maHoaDon)
				.Include(o => o.ChiTietHoaDons)
				.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<HoaDon>> GetByNameAndCccdAsync(string tenKhachHang, string cccd, CancellationToken cancellationToken = default)
		{
			return await _orderRepo.Query()
				.Where(o => o.TenKhachHang == tenKhachHang && o.Cccd == cccd)
				.OrderByDescending(o => o.CreatedDate)
				.ToListAsync(cancellationToken);
		}
	}
}


