using System;
using System.Collections.Generic;
using System.Linq;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;

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

			// Tải sản phẩm một lần để tránh truy vấn lặp
			var productCache = new Dictionary<string, Product>();
			foreach (var it in req.Items)
			{
				var product = await _productRepo.GetByIdAsync(it.ProductId, cancellationToken) 
				              ?? throw new InvalidOperationException($"Sản phẩm {it.ProductId} không tồn tại.");
				if (product.SoLuong < it.Quantity)
				{
					throw new InvalidOperationException($"Sản phẩm {product.Title} không đủ tồn kho. Còn lại: {product.SoLuong}");
				}
				productCache[it.ProductId] = product;
			}

			HoaDon order = new HoaDon
			{
				TenKhachHang = req.TenKhachHang,
				SoDienThoai = req.SoDienThoai,
				DiaChi = req.DiaChi,
				Email = req.Email,
				Cccd = string.Empty,
				TrangThai = 1,
				PhuongThucThanhToan = req.TypePayment,
				CreatedDate = DateTime.Now,
				ModifiedDate = DateTime.Now,
				CreatedBy = req.TenKhachHang
			};

			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			string currentYear = DateTime.Now.ToString("yy");
			string currentMonth = DateTime.Now.ToString("MM");
			string currentDay = DateTime.Now.ToString("dd");
			string randomString = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
			order.MaHoaDon = currentYear + currentMonth + currentDay + randomString;

			foreach (var it in req.Items)
			{
				var product = productCache[it.ProductId];
				order.ChiTietHoaDons.Add(new ChiTietHoaDon
				{
					OrderId = order.MaHoaDon,
					ProductId = product.MaSanPham,
					SoLuong = it.Quantity,
					GiaBan = product.GiaBan ?? product.GiaNiemYet,
					GiamGia = (int)(product.GiamGia ?? 0)
				});

				product.SoLuong = Math.Max(0, product.SoLuong - it.Quantity);
				_productRepo.Update(product);
			}

			_orderRepo.Add(order);
			await _orderRepo.SaveChangesAsync(cancellationToken);
			return order;
		}
	}

	public class VnPayService : IPaymentService
	{
		public string CreateVnPayUrl(HoaDon order, int typePaymentVN, string returnUrl, string vnpUrl, string tmnCode, string hashSecret, string clientIp)
		{
			decimal tong = order.ChiTietHoaDons.Sum(x => x.SoLuong * x.GiaBan);
			long amount = (long)(tong * 100);
			var dict = new SortedDictionary<string, string>(StringComparer.Ordinal)
			{
				["vnp_Version"] = "2.1.0",
				["vnp_Command"] = "pay",
				["vnp_TmnCode"] = tmnCode,
				["vnp_Amount"] = amount.ToString(),
				["vnp_CreateDate"] = order.CreatedDate.ToString("yyyyMMddHHmmss"),
				["vnp_CurrCode"] = "VND",
				["vnp_IpAddr"] = string.IsNullOrWhiteSpace(clientIp) ? "127.0.0.1" : clientIp,
				["vnp_Locale"] = "vn",
				["vnp_OrderInfo"] = $"Thanh toán đơn hàng: {order.MaHoaDon}",
				["vnp_OrderType"] = "other",
				["vnp_ReturnUrl"] = returnUrl,
				["vnp_TxnRef"] = order.MaHoaDon
			};
			if (typePaymentVN == 1) dict["vnp_BankCode"] = "VNPAYQR";
			else if (typePaymentVN == 2) dict["vnp_BankCode"] = "VNBANK";
			else if (typePaymentVN == 3) dict["vnp_BankCode"] = "INTCARD";

			var rawData = string.Join("&", dict.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
			string secureHash = ComputeHmacSha512(hashSecret ?? string.Empty, rawData);
			var query = rawData + "&vnp_SecureHashType=HMACSHA512&vnp_SecureHash=" + secureHash;
			return $"{vnpUrl}?{query}";
		}

		private static string ComputeHmacSha512(string secret, string data)
		{
			if (string.IsNullOrWhiteSpace(secret)) return string.Empty;
			using var h = new System.Security.Cryptography.HMACSHA512(System.Text.Encoding.UTF8.GetBytes(secret));
			var bytes = h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
			return BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
		}
	}
}

