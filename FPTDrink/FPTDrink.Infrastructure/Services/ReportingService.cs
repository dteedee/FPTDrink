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

		public async Task<IReadOnlyList<FPTDrink.Core.Models.Reports.RevenuePoint>> GetStatisticalAsync(DateTime fromDay, DateTime toDay, CancellationToken cancellationToken = default)
		{
			var query = from o in _db.HoaDons
						join od in _db.ChiTietHoaDons on o.MaHoaDon equals od.OrderId
						join p in _db.Products on od.ProductId equals p.MaSanPham
						where o.TrangThai == 2
						select new
						{
							CreatedDate = o.CreatedDate,
							Quantity = od.SoLuong,
							Price = od.GiaBan,
							OriginalPrice = p.GiaNhap
						};
			query = query.Where(x => x.CreatedDate >= fromDay && x.CreatedDate < toDay);

			var result = await query
				.GroupBy(x => x.CreatedDate.Date)
				.Select(x => new FPTDrink.Core.Models.Reports.RevenuePoint
				{
					Date = x.Key,
					DoanhThu = x.Sum(y => y.Quantity * y.Price),
					LoiNhuan = x.Sum(y => y.Quantity * (y.Price - y.OriginalPrice))
				}).ToListAsync(cancellationToken);
			return result;
		}

		public async Task<IReadOnlyList<FPTDrink.Core.Models.Reports.ProductSalesPoint>> GetProductSalesAsync(DateTime fromDay, DateTime toDay, CancellationToken cancellationToken = default)
		{
			var query = from o in _db.HoaDons
						join od in _db.ChiTietHoaDons on o.MaHoaDon equals od.OrderId
						where o.TrangThai == 2
						select new
						{
							CreatedDate = o.CreatedDate,
							Quantity = od.SoLuong,
							ProductName = od.Product != null ? od.Product.Title : ""
						};
			query = query.Where(x => x.CreatedDate >= fromDay && x.CreatedDate < toDay);

			var list = await query
				.GroupBy(x => x.CreatedDate.Date)
				.Select(x => new
				{
					Date = x.Key,
					TotalProducts = x.Sum(y => y.Quantity),
					Products = x.GroupBy(y => y.ProductName)
								.Select(y => new FPTDrink.Core.Models.Reports.ProductQuantity
								{
									ProductName = y.Key,
									Quantity = y.Sum(z => z.Quantity)
								})
								.OrderByDescending(y => y.Quantity)
								.ThenBy(y => y.ProductName)
								.Take(5)
								.ToList(),
					BestOrdered = x.GroupBy(y => y.ProductName)
								   .Select(y => new
								   {
									   ProductName = y.Key,
									   Quantity = y.Sum(z => z.Quantity)
								   })
								   .OrderByDescending(y => y.Quantity)
								   .ThenBy(y => y.ProductName)
								   .ToList()
				}).ToListAsync(cancellationToken);

			var shaped = list.Select(x => new FPTDrink.Core.Models.Reports.ProductSalesPoint
			{
				Date = x.Date,
				TotalProducts = x.TotalProducts,
				Products = x.Products,
				BestSellingProduct = (x.BestOrdered.Count > 1 && x.BestOrdered[0].Quantity == x.BestOrdered[1].Quantity)
					? "Chưa xác định" : x.BestOrdered[0].ProductName
			}).ToList();

			return shaped;
		}

		public async Task<IReadOnlyList<FPTDrink.Core.Models.Reports.ProductSalesDetailItem>> GetProductSalesDetailAsync(DateTime date, CancellationToken cancellationToken = default)
		{
			var productSales = await _db.ChiTietHoaDons
				.Where(x => x.Order != null && x.Order.CreatedDate.Date == date.Date && x.Order.TrangThai == 2)
				.GroupBy(x => x.Product!.Title)
				.Select(group => new FPTDrink.Core.Models.Reports.ProductSalesDetailItem
				{
					ProductID = group.FirstOrDefault()!.ProductId,
					ProductName = group.Key,
					Quantity = group.Sum(x => x.SoLuong),
					UnitPrice = group.FirstOrDefault()!.GiaBan,
					GiaNhap = group.FirstOrDefault()!.Product!.GiaNhap
				}).ToListAsync(cancellationToken);

			return productSales;
		}

		public async Task<IReadOnlyList<FPTDrink.Core.Models.Reports.PaymentMethodStatPoint>> GetPaymentMethodsStatisticalAsync(DateTime fromDay, DateTime toDay, CancellationToken cancellationToken = default)
		{
			var query = _db.HoaDons.Select(o => new
			{
				o.CreatedDate,
				PaymentMethod = o.PhuongThucThanhToan
			});
			query = query.Where(x => x.CreatedDate >= fromDay && x.CreatedDate < toDay);

			var result = await query
				.GroupBy(x => x.CreatedDate.Date)
				.Select(g => new FPTDrink.Core.Models.Reports.PaymentMethodStatPoint
				{
					Date = g.Key,
					OnlineCount = g.Count(x => x.PaymentMethod == 1 || x.PaymentMethod == 2),
					OfflineCount = g.Count(x => x.PaymentMethod == 3)
				}).ToListAsync(cancellationToken);

			return result;
		}

		public async Task<IReadOnlyList<FPTDrink.Core.Models.Reports.InvoiceBrief>> GetPaymentMethodDetailAsync(DateTime date, CancellationToken cancellationToken = default)
		{
			var invoices = await _db.HoaDons
				.Where(x => x.CreatedDate.Date == date.Date && x.TrangThai == 2)
				.Select(x => new FPTDrink.Core.Models.Reports.InvoiceBrief
				{
					MaHoaDon = x.MaHoaDon,
					CreatedDate = x.CreatedDate,
					ID_KhachHang = x.IdKhachHang,
					TenKhachHang = x.TenKhachHang,
					PhuongThucThanhToan = x.PhuongThucThanhToan,
					TrangThai = x.TrangThai,
					TongHoaDon = x.ChiTietHoaDons.Sum(ct => (decimal?)ct.SoLuong * ct.GiaBan) ?? 0m
				})
				.ToListAsync(cancellationToken);

			return invoices;
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
				: today.AddDays(-(days - 1)); // Để có 7 ngày bao gồm cả hôm nay: từ 6 ngày trước đến hôm nay

			for (int i = 0; i < days; i++)
			{
				var date = startDate.AddDays(i);
				if (string.Equals(period, "thisMonth", StringComparison.OrdinalIgnoreCase) && date > today) break;
				if (date > today) break; // Không lấy ngày trong tương lai
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


