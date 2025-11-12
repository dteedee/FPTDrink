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
				.ThenInclude(ct => ct.Product)
				.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<HoaDon>> GetByNameAndPhoneAsync(string tenKhachHang, string soDienThoai, CancellationToken cancellationToken = default)
		{
			return await _orderRepo.Query()
				.Where(o => o.TenKhachHang == tenKhachHang && o.SoDienThoai == soDienThoai)
				.Include(o => o.ChiTietHoaDons)
				.ThenInclude(ct => ct.Product)
				.OrderByDescending(o => o.CreatedDate)
				.ToListAsync(cancellationToken);
		}
	}
}


