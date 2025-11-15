using System.Linq;
using FPTDrink.Web.ViewModels;
using System.Text.Json;

namespace FPTDrink.Web.Helpers
{
	public static class OrderMappingHelper
	{
		/// <summary>
		/// Maps JSON from API (OrderDetailDto structure) to OrderDetailViewModel
		/// This handles the case where API returns OrderDetailDto but we need OrderDetailViewModel
		/// </summary>
		public static OrderDetailViewModel? MapFromJson(JsonElement jsonElement)
		{
			try
			{
				var order = new OrderDetailViewModel
				{
					MaHoaDon = jsonElement.GetProperty("maHoaDon").GetString() ?? string.Empty,
					IdKhachHang = jsonElement.TryGetProperty("idKhachHang", out var idKhachHang) ? idKhachHang.GetString() : null,
					TenKhachHang = jsonElement.GetProperty("tenKhachHang").GetString() ?? string.Empty,
					SoDienThoai = jsonElement.GetProperty("soDienThoai").GetString() ?? string.Empty,
					DiaChi = jsonElement.GetProperty("diaChi").GetString() ?? string.Empty,
					Email = jsonElement.TryGetProperty("email", out var email) ? email.GetString() : null,
					PhuongThucThanhToan = jsonElement.GetProperty("phuongThucThanhToan").GetInt32(),
					TrangThai = jsonElement.GetProperty("trangThai").GetInt32(),
					CreatedDate = jsonElement.GetProperty("createdDate").GetDateTime()
				};

				if (jsonElement.TryGetProperty("chiTietHoaDons", out var chiTietHoaDons) && chiTietHoaDons.ValueKind == JsonValueKind.Array)
				{
					order.ChiTietHoaDons = chiTietHoaDons.EnumerateArray().Select(item =>
					{
						var orderItem = new OrderItemViewModel
						{
							ProductId = item.TryGetProperty("productId", out var productId) ? productId.GetString() : null,
							GiaBan = item.GetProperty("giaBan").GetDecimal(),
							GiamGia = item.GetProperty("giamGia").GetInt32(),
							SoLuong = item.GetProperty("soLuong").GetInt32()
						};

						if (item.TryGetProperty("product", out var product) && product.ValueKind == JsonValueKind.Object)
						{
							orderItem.Product = new OrderItemProductViewModel
							{
								MaSanPham = product.TryGetProperty("maSanPham", out var maSanPham) ? maSanPham.GetString() : null,
								Title = product.TryGetProperty("title", out var title) ? title.GetString() : null,
								Image = product.TryGetProperty("image", out var image) ? image.GetString() : null
							};
						}

						return orderItem;
					}).ToList();
				}

				return order;
			}
			catch
			{
				return null;
			}
		}
	}
}

