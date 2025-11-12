using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
			var vnpay = new VnPayLibrary();
			vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
			vnpay.AddRequestData("vnp_Command", "pay");
			vnpay.AddRequestData("vnp_TmnCode", tmnCode);
			vnpay.AddRequestData("vnp_Amount", amount.ToString(CultureInfo.InvariantCulture));
			vnpay.AddRequestData("vnp_CreateDate", order.CreatedDate.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
			vnpay.AddRequestData("vnp_CurrCode", "VND");
			vnpay.AddRequestData("vnp_IpAddr", string.IsNullOrWhiteSpace(clientIp) ? "127.0.0.1" : clientIp);
			vnpay.AddRequestData("vnp_Locale", "vn");
			vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toán đơn hàng: {order.MaHoaDon}");
			vnpay.AddRequestData("vnp_OrderType", "other");
			vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
			vnpay.AddRequestData("vnp_TxnRef", order.MaHoaDon);
			if (typePaymentVN == 2) vnpay.AddRequestData("vnp_BankCode", "VNBANK");
			else if (typePaymentVN == 3) vnpay.AddRequestData("vnp_BankCode", "INTCARD");

			return vnpay.CreateRequestUrl(vnpUrl, hashSecret ?? string.Empty);
		}
	}

	public class VnPayLibrary
	{
		public const string VERSION = "2.1.0";
		private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());

		public void AddRequestData(string key, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				_requestData[key] = value;
			}
		}

		public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
		{
			var data = new StringBuilder();
			foreach (var kv in _requestData)
			{
				data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
			}

			if (data.Length > 0)
			{
				data.Length -= 1;
			}

			string query = data.ToString();
			string secureHash = Utils.HmacSHA512(vnpHashSecret, query);

			var sb = new StringBuilder();
			sb.Append(baseUrl);
			sb.Append("?");
			sb.Append(query);
			sb.Append("&vnp_SecureHashType=HMACSHA512&vnp_SecureHash=");
			sb.Append(secureHash);
			return sb.ToString();
		}
	}

	public static class Utils
	{
		public static string HmacSHA512(string key, string inputData)
		{
			if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(inputData))
			{
				return string.Empty;
			}

			var hash = new StringBuilder();
			byte[] keyBytes = Encoding.UTF8.GetBytes(key);
			byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
			using (var hmac = new HMACSHA512(keyBytes))
			{
				byte[] hashValue = hmac.ComputeHash(inputBytes);
				foreach (var theByte in hashValue)
				{
					hash.Append(theByte.ToString("x2", CultureInfo.InvariantCulture));
				}
			}
			return hash.ToString().ToUpperInvariant();
		}
	}

	public class VnPayCompare : IComparer<string>
	{
		public int Compare(string? x, string? y)
		{
			if (x == y) return 0;
			if (x == null) return -1;
			if (y == null) return 1;
			var vnpCompare = CompareInfo.GetCompareInfo("en-US");
			return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
		}
	}
}

