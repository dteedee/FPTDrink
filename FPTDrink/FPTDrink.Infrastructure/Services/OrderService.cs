using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace FPTDrink.Infrastructure.Services
{
	public class OrderService : IOrderService
	{
		private readonly IHoaDonRepository _orderRepo;
		private readonly IChiTietHoaDonRepository _itemRepo;
		private readonly IProductRepository _productRepo;

		public OrderService(IHoaDonRepository orderRepo, IChiTietHoaDonRepository itemRepo, IProductRepository productRepo)
		{
			_orderRepo = orderRepo;
			_itemRepo = itemRepo;
			_productRepo = productRepo;
		}

		public async Task<IReadOnlyList<HoaDon>> GetListAsync(string? search, CancellationToken cancellationToken = default)
		{
			IQueryable<HoaDon> q = _orderRepo.Query().OrderByDescending(x => x.CreatedDate);
			if (!string.IsNullOrWhiteSpace(search))
			{
				if (DateTime.TryParseExact(search, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
				{
					q = q.Where(c => c.CreatedDate.Date == d.Date);
				}
				else
				{
					q = q.Where(c =>
						c.MaHoaDon.Contains(search) ||
						c.TenKhachHang.Contains(search) ||
						c.Cccd.Contains(search));
				}
			}
			return await q.ToListAsync(cancellationToken);
		}

		public Task<HoaDon?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _orderRepo.GetByIdAsync(id, cancellationToken);
		}

		public Task<IReadOnlyList<ChiTietHoaDon>> GetItemsAsync(string id, CancellationToken cancellationToken = default)
		{
			return _itemRepo.GetByOrderIdAsync(id, cancellationToken);
		}

		public async Task<(bool success, string message)> UpdateStatusAsync(string id, int newStatus, bool confirmed, CancellationToken cancellationToken = default)
		{
			var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
			if (order == null) return (false, "Không tìm thấy đơn hàng");
			int oldStatus = order.TrangThai;
			if (oldStatus == 2 || oldStatus == 3)
			{
				if (!confirmed)
				{
					return (false, $"Đơn hàng đang ở trạng thái '{GetStatusName(oldStatus)}'. Trạng thái này không thể thay đổi.");
				}
				return (false, $"Đơn hàng ở trạng thái '{GetStatusName(oldStatus)}' đã cố định và không thể thay đổi.");
			}
			if (oldStatus != 1) return (false, "Chỉ có thể thay đổi từ 'Chờ thanh toán'.");
			if (newStatus != 2 && newStatus != 3) return (false, "Chỉ cho phép chuyển sang 'Đã thanh toán' hoặc 'Đã huỷ'.");

			// update status
			order.TrangThai = newStatus;
			order.ModifiedDate = DateTime.Now;
			_orderRepo.Update(order);

			// if cancel, restore inventory
			if (newStatus == 3)
			{
				var items = await _itemRepo.GetByOrderIdAsync(order.MaHoaDon, cancellationToken);
				foreach (var detail in items)
				{
					if (detail.ProductId == null) continue;
					var product = await _productRepo.GetByIdAsync(detail.ProductId, cancellationToken);
					if (product != null)
					{
						product.SoLuong += detail.SoLuong;
						_productRepo.Update(product);
					}
				}
			}
			await _orderRepo.SaveChangesAsync(cancellationToken);
			await _productRepo.SaveChangesAsync(cancellationToken);
			return (true, "Cập nhật trạng thái thành công");
		}

		public async Task<IReadOnlyList<FPTDrink.Core.Models.Reports.CustomerSummaryInfo>> GetCustomersAsync(string? search, CancellationToken cancellationToken = default)
		{
			var customers = await _orderRepo.Query()
				.GroupBy(h => new { h.IdKhachHang, h.TenKhachHang })
				.Select(g => new FPTDrink.Core.Models.Reports.CustomerSummaryInfo
				{
					MaKhachHang = g.Key.IdKhachHang,
					TenKhachHang = g.Key.TenKhachHang,
					Email = g.OrderByDescending(h => h.CreatedDate).FirstOrDefault()!.Email,
					SoDienThoai = g.OrderByDescending(h => h.CreatedDate).FirstOrDefault()!.SoDienThoai
				})
				.ToListAsync(cancellationToken);
			if (!string.IsNullOrWhiteSpace(search))
			{
				customers = customers
					.Where(c => (c.MaKhachHang ?? "").Contains(search) || (c.TenKhachHang ?? "").Contains(search) || (c.Email ?? "").Contains(search) || (c.SoDienThoai ?? "").Contains(search))
					.ToList();
			}
			return customers;
		}

		public async Task<FPTDrink.Core.Models.Reports.CustomerDetailsInfo?> GetCustomerDetailsAsync(string id, CancellationToken cancellationToken = default)
		{
			var latest = await _orderRepo.Query()
				.Where(h => h.IdKhachHang == id)
				.OrderByDescending(h => h.CreatedDate)
				.FirstOrDefaultAsync(cancellationToken);
			if (latest == null) return null;
			var orders = await _orderRepo.Query()
				.Where(d => d.IdKhachHang == id)
				.Select(d => new FPTDrink.Core.Models.Reports.CustomerOrderBrief
				{
					MaHoaDon = d.MaHoaDon,
					ThoiGianLap = d.CreatedDate,
					SoDienThoai = d.SoDienThoai,
					Email = d.Email,
					DiaChi = d.DiaChi,
					Status = d.TrangThai
				}).ToListAsync(cancellationToken);
			return new FPTDrink.Core.Models.Reports.CustomerDetailsInfo
			{
				MaKhachHang = id,
				TenKhachHang = latest.TenKhachHang,
				HoaDons = orders
			};
		}

		private static string GetStatusName(int status) => status switch
		{
			1 => "Chờ thanh toán",
			2 => "Đã thanh toán",
			3 => "Đã huỷ",
			_ => "Không xác định"
		};
	}
}


