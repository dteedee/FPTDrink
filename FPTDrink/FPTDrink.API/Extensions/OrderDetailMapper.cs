using System.Collections.Generic;
using FPTDrink.API.DTOs.Public.Checkout;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Extensions
{
	public static class OrderDetailMapper
	{
		public static OrderDetailDto ToDto(HoaDon order)
		{
			var dto = new OrderDetailDto
			{
				MaHoaDon = order.MaHoaDon,
				TenKhachHang = order.TenKhachHang,
				SoDienThoai = order.SoDienThoai,
				DiaChi = order.DiaChi,
				Email = order.Email,
				PhuongThucThanhToan = order.PhuongThucThanhToan,
				TrangThai = order.TrangThai,
				CreatedDate = order.CreatedDate
			};

			foreach (var item in order.ChiTietHoaDons)
			{
				dto.ChiTietHoaDons.Add(new OrderDetailItemDto
				{
					ProductId = item.ProductId,
					GiaBan = item.GiaBan,
					GiamGia = item.GiamGia,
					SoLuong = item.SoLuong,
					Product = item.Product != null ? new OrderDetailProductDto
					{
						MaSanPham = item.Product.MaSanPham,
						Title = item.Product.Title,
						Image = item.Product.Image
					} : null
				});
			}

			return dto;
		}

		public static List<OrderDetailDto> ToDtos(IEnumerable<HoaDon> orders)
		{
			var list = new List<OrderDetailDto>();
			foreach (var order in orders)
			{
				list.Add(ToDto(order));
			}
			return list;
		}
	}
}

