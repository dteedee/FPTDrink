using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class CheckoutService : ICheckoutService
	{
		private readonly IHoaDonRepository _orderRepo;
		private readonly IProductRepository _productRepo;

		public CheckoutService(IHoaDonRepository orderRepo, IProductRepository productRepo)
		{
			_orderRepo = orderRepo;
			_productRepo = productRepo;
		}

		public async Task<HoaDon> CreateOrderAsync(CreateOrderRequest req, CancellationToken cancellationToken = default)
		{
			if (req.Items.Count == 0) throw new InvalidOperationException("Giỏ hàng trống.");
			foreach (var it in req.Items)
			{
				var p = await _productRepo.GetByIdAsync(it.ProductId, cancellationToken) ?? throw new InvalidOperationException($"Sản phẩm {it.ProductId} không tồn tại.");
				if (p.SoLuong < it.Quantity) throw new InvalidOperationException($"Sản phẩm {p.Title} không đủ tồn kho. Còn lại: {p.SoLuong}");
			}

			HoaDon order = new HoaDon
			{
				TenKhachHang = req.TenKhachHang,
				SoDienThoai = req.SoDienThoai,
				DiaChi = req.DiaChi,
				Email = req.Email,
				Cccd = req.CCCD,
				TrangThai = 1,
				PhuongThucThanhToan = req.TypePayment,
				CreatedDate = DateTime.Now,
				ModifiedDate = DateTime.Now,
				CreatedBy = req.TenKhachHang
			};

			// Tạo mã đơn
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			string currentYear = DateTime.Now.ToString("yy");
			string currentMonth = DateTime.Now.ToString("MM");
			string currentDay = DateTime.Now.ToString("dd");
			string randomString = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
			order.MaHoaDon = currentYear + currentMonth + currentDay + randomString;

			// Gán chi tiết
			foreach (var it in req.Items)
			{
				var p = await _productRepo.GetByIdAsync(it.ProductId, cancellationToken);
				order.ChiTietHoaDons.Add(new ChiTietHoaDon
				{
					OrderId = order.MaHoaDon,
					ProductId = p!.MaSanPham,
					SoLuong = it.Quantity,
					GiaBan = p.GiaBan ?? p.GiaNiemYet,
					GiamGia = 0
				});
			}

			// Cập nhật tồn kho (theo logic cũ)
			foreach (var it in req.Items)
			{
				var p = await _productRepo.GetByIdAsync(it.ProductId, cancellationToken);
				p!.SoLuong = Math.Max(0, p.SoLuong - it.Quantity);
				_productRepo.Update(p);
			}
			await _productRepo.SaveChangesAsync(cancellationToken);

			// Lưu đơn
			await _orderRepo.SaveChangesAsync(cancellationToken); // ensure context
			_orderRepo.Update(order); // dùng Update để attach rồi save
			await _orderRepo.SaveChangesAsync(cancellationToken);
			return order;
		}
	}

	public class VnPayService : IPaymentService
	{
		public string CreateVnPayUrl(HoaDon order, int typePaymentVN, string returnUrl, string vnpUrl, string tmnCode, string hashSecret)
		{
			// Đơn giản hoá: ghép query theo chuẩn VNPay (không dùng SDK)
			// vnp_Amount cần *100
			decimal tong = order.ChiTietHoaDons.Sum(x => x.SoLuong * x.GiaBan);
			long amount = (long)(tong * 100);
			var dict = new SortedDictionary<string, string>
			{
				["vnp_Version"] = "2.1.0",
				["vnp_Command"] = "pay",
				["vnp_TmnCode"] = tmnCode,
				["vnp_Amount"] = amount.ToString(),
				["vnp_CreateDate"] = order.CreatedDate.ToString("yyyyMMddHHmmss"),
				["vnp_CurrCode"] = "VND",
				["vnp_IpAddr"] = "127.0.0.1",
				["vnp_Locale"] = "vn",
				["vnp_OrderInfo"] = $"Thanh toán đơn hàng: {order.MaHoaDon}",
				["vnp_OrderType"] = "other",
				["vnp_ReturnUrl"] = returnUrl,
				["vnp_TxnRef"] = order.MaHoaDon
			};
			if (typePaymentVN == 1) dict["vnp_BankCode"] = "VNPAYQR";
			else if (typePaymentVN == 2) dict["vnp_BankCode"] = "VNBANK";
			else if (typePaymentVN == 3) dict["vnp_BankCode"] = "INTCARD";

			// Tạo chuỗi ký
			var query = string.Join("&", dict.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
			// Bỏ qua ký chữ ký để đơn giản (có thể thêm HMAC SHA512 với hashSecret)
			var url = $"{vnpUrl}?{query}";
			return url;
		}
	}
}

