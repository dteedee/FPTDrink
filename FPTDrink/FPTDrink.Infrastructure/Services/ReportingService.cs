using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class ReportingService : IReportingService
	{
		private readonly FptdrinkContext _db;

		public ReportingService(FptdrinkContext db)
		{
			_db = db;
		}

		public async Task<object> GetOverviewStatsAsync(CancellationToken cancellationToken = default)
		{
			var today = DateTime.Now.Date;
			var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			var lastMonth = thisMonth.AddMonths(-1);

			decimal totalRevenue = await _db.HoaDons
				.Where(x => x.TrangThai == 2)
				.SelectMany(x => x.ChiTietHoaDons)
				.SumAsync(x => (decimal?)x.SoLuong * x.GiaBan, cancellationToken) ?? 0;

			decimal todayRevenue = await _db.HoaDons
				.Where(x => x.TrangThai == 2 && x.CreatedDate.Date == today)
				.SelectMany(x => x.ChiTietHoaDons)
				.SumAsync(x => (decimal?)x.SoLuong * x.GiaBan, cancellationToken) ?? 0;

			decimal thisMonthRevenue = await _db.HoaDons
				.Where(x => x.TrangThai == 2 && x.CreatedDate >= thisMonth)
				.SelectMany(x => x.ChiTietHoaDons)
				.SumAsync(x => (decimal?)x.SoLuong * x.GiaBan, cancellationToken) ?? 0;

			decimal lastMonthRevenue = await _db.HoaDons
				.Where(x => x.TrangThai == 2 && x.CreatedDate >= lastMonth && x.CreatedDate < thisMonth)
				.SelectMany(x => x.ChiTietHoaDons)
				.SumAsync(x => (decimal?)x.SoLuong * x.GiaBan, cancellationToken) ?? 0;

			int totalOrders = await _db.HoaDons.CountAsync(cancellationToken);
			int todayOrders = await _db.HoaDons.CountAsync(x => x.CreatedDate.Date == today, cancellationToken);
			int paidOrders = await _db.HoaDons.CountAsync(x => x.TrangThai == 2, cancellationToken);
			int pendingOrders = await _db.HoaDons.CountAsync(x => x.TrangThai == 1, cancellationToken);

			int totalProducts = await _db.Products.CountAsync(x => x.Status != 0 && x.IsActive == true, cancellationToken);
			int outOfStockProducts = await _db.Products.CountAsync(x => x.Status != 0 && x.IsActive == true && x.SoLuong <= 0, cancellationToken);

			int totalCustomers = await _db.HoaDons.GroupBy(x => x.IdKhachHang).CountAsync(cancellationToken);
			int newCustomersThisMonth = await _db.HoaDons.Where(x => x.CreatedDate >= thisMonth).GroupBy(x => x.IdKhachHang).CountAsync(cancellationToken);

			int totalCategories = await _db.ProductCategories.CountAsync(x => x.Status != 0 && x.IsActive == true, cancellationToken);
			int totalSuppliers = await _db.NhaCungCaps.CountAsync(x => x.Status != 0, cancellationToken);
			int totalEmployees = await _db.NhanViens.CountAsync(x => x.Status != 0 && x.IsActiveAccount == true, cancellationToken);

			return new
			{
				Revenue = new
				{
					Total = totalRevenue,
					Today = todayRevenue,
					ThisMonth = thisMonthRevenue,
					LastMonth = lastMonthRevenue,
					Growth = lastMonthRevenue > 0 ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue * 100) : 0
				},
				Orders = new
				{
					Total = totalOrders,
					Today = todayOrders,
					Paid = paidOrders,
					Pending = pendingOrders
				},
				Products = new
				{
					Total = totalProducts,
					OutOfStock = outOfStockProducts
				},
				Customers = new
				{
					Total = totalCustomers,
					NewThisMonth = newCustomersThisMonth
				},
				Categories = totalCategories,
				Suppliers = totalSuppliers,
				Employees = totalEmployees
			};
		}

		public async Task<IReadOnlyList<object>> GetRevenueChartAsync(string period, CancellationToken cancellationToken = default)
		{
			var today = DateTime.Now.Date;
			var result = new List<object>();

			if (string.Equals(period, "thisYear", StringComparison.OrdinalIgnoreCase))
			{
				var start = new DateTime(DateTime.Now.Year, 1, 1);
				for (int i = 0; i < 12; i++)
				{
					var monthStart = start.AddMonths(i);
					var monthEnd = monthStart.AddMonths(1);
					decimal rev = await _db.HoaDons
						.Where(x => x.TrangThai == 2 && x.CreatedDate >= monthStart && x.CreatedDate < monthEnd)
						.SelectMany(x => x.ChiTietHoaDons)
						.SumAsync(x => (decimal?)x.SoLuong * x.GiaBan, cancellationToken) ?? 0;
					result.Add(new { Date = monthStart.ToString("MM/yyyy"), Revenue = rev });
				}
				return result;
			}

			int days = 7;
			if (string.Equals(period, "30days", StringComparison.OrdinalIgnoreCase)) days = 30;
			else if (string.Equals(period, "thisMonth", StringComparison.OrdinalIgnoreCase)) days = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);

			var startDate = string.Equals(period, "thisMonth", StringComparison.OrdinalIgnoreCase)
				? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
				: today.AddDays(-days);

			for (int i = 0; i < days; i++)
			{
				var date = startDate.AddDays(i);
				if (string.Equals(period, "thisMonth", StringComparison.OrdinalIgnoreCase) && date > today) break;
				decimal rev = await _db.HoaDons
					.Where(x => x.TrangThai == 2 && x.CreatedDate.Date == date)
					.SelectMany(x => x.ChiTietHoaDons)
					.SumAsync(x => (decimal?)x.SoLuong * x.GiaBan, cancellationToken) ?? 0;
				result.Add(new { Date = date.ToString("dd/MM"), Revenue = rev });
			}

			return result;
		}

		public async Task<IReadOnlyList<object>> GetTopProductsAsync(int top, CancellationToken cancellationToken = default)
		{
			var data = await _db.ChiTietHoaDons
				.Where(x => x.Order != null && x.Order.TrangThai == 2)
				.GroupBy(x => new { x.ProductId, x.Product!.Title, x.Product.Image })
				.Select(g => new
				{
					ProductID = g.Key.ProductId,
					ProductName = g.Key.Title,
					ProductImage = g.Key.Image,
					TotalQuantity = g.Sum(x => x.SoLuong),
					TotalRevenue = g.Sum(x => x.SoLuong * x.GiaBan)
				})
				.OrderByDescending(x => x.TotalQuantity)
				.Take(top)
				.ToListAsync(cancellationToken);

			return data.Cast<object>().ToList();
		}

		public async Task<IReadOnlyList<object>> GetRecentOrdersAsync(int count, CancellationToken cancellationToken = default)
		{
			var orders = await _db.HoaDons
				.OrderByDescending(x => x.CreatedDate)
				.Take(count)
				.Select(x => new
				{
					MaHoaDon = x.MaHoaDon,
					TenKhachHang = x.TenKhachHang,
					CreatedDate = x.CreatedDate,
					TrangThai = x.TrangThai,
					TongTien = x.ChiTietHoaDons.Sum(ct => (decimal?)ct.SoLuong * ct.GiaBan) ?? 0,
					PhuongThucThanhToan = x.PhuongThucThanhToan
				})
				.ToListAsync(cancellationToken);

			return orders.Cast<object>().ToList();
		}
	}
}


