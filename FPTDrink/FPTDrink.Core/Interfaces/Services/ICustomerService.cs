using System;
using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface ICustomerService
	{
		Task<KhachHang?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
		Task<KhachHang?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> UpdateProfileAsync(string customerId, UpdateCustomerProfileRequest request, CancellationToken cancellationToken = default);
		Task<bool> IsEmailVerifiedAsync(string customerId, CancellationToken cancellationToken = default);
	}

	public class UpdateCustomerProfileRequest
	{
		public string? HoTen { get; set; }
		public string? SoDienThoai { get; set; }
		public DateTime? NgaySinh { get; set; }
		public bool? GioiTinh { get; set; }
		public string? DiaChi { get; set; }
		public string? Image { get; set; }
	}
}

