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
			
			// Không cho phép chuyển nếu trạng thái mới giống trạng thái cũ
			if (oldStatus == newStatus)
			{
				return (false, "Trạng thái mới phải khác trạng thái hiện tại.");
			}

			// Quy trình chuyển trạng thái:
			// 0: Đã hủy - không thể chuyển sang trạng thái khác
			// 1: Đang xử lý - có thể chuyển sang: Đang giao (3) hoặc Chờ thanh toán (4)
			// 2: Hoàn tất - không thể chuyển sang trạng thái khác
			// 3: Đang giao - có thể chuyển sang: Hoàn tất (2) hoặc Đã hủy (0)
			// 4: Chờ thanh toán - có thể chuyển sang: Đang giao (3) hoặc Đã hủy (0)

			bool isValidTransition = false;
			string errorMessage = string.Empty;

			switch (oldStatus)
			{
				case 0: // Đã hủy
					return (false, "Đơn hàng đã hủy không thể thay đổi trạng thái.");
				
				case 1: // Đang xử lý
					if (newStatus == 3 || newStatus == 4)
					{
						isValidTransition = true;
					}
					else
					{
						errorMessage = "Từ trạng thái 'Đang xử lý' chỉ có thể chuyển sang 'Đang giao' hoặc 'Chờ thanh toán'.";
					}
					break;
				
				case 2: // Hoàn tất
					return (false, "Đơn hàng đã hoàn tất không thể thay đổi trạng thái.");
				
				case 3: // Đang giao
					if (newStatus == 2 || newStatus == 0)
					{
						isValidTransition = true;
					}
					else
					{
						errorMessage = "Từ trạng thái 'Đang giao' chỉ có thể chuyển sang 'Hoàn tất' hoặc 'Đã hủy'.";
					}
					break;
				
				case 4: // Chờ thanh toán
					if (newStatus == 3 || newStatus == 0)
					{
						isValidTransition = true;
					}
					else
					{
						errorMessage = "Từ trạng thái 'Chờ thanh toán' chỉ có thể chuyển sang 'Đang giao' hoặc 'Đã hủy'.";
					}
					break;
				
				default:
					return (false, "Trạng thái hiện tại không hợp lệ.");
			}

			if (!isValidTransition)
			{
				return (false, errorMessage);
			}

			// Cập nhật trạng thái
			order.TrangThai = newStatus;
			order.ModifiedDate = DateTime.Now;
			_orderRepo.Update(order);

			// Nếu hủy đơn, hoàn trả số lượng sản phẩm
			if (newStatus == 0)
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
				await _productRepo.SaveChangesAsync(cancellationToken);
			}

			await _orderRepo.SaveChangesAsync(cancellationToken);
			return (true, $"Đã cập nhật trạng thái từ '{GetStatusName(oldStatus)}' sang '{GetStatusName(newStatus)}' thành công.");
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
			0 => "Đã hủy",
			1 => "Đang xử lý",
			2 => "Hoàn tất",
			3 => "Đang giao",
			4 => "Chờ thanh toán",
			_ => "Không xác định"
		};
	}
}


