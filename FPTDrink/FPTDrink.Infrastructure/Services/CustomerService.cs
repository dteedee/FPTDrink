using System;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;

namespace FPTDrink.Infrastructure.Services
{
	public class CustomerService : ICustomerService
	{
		private readonly IKhachHangRepository _khachHangRepository;

		public CustomerService(IKhachHangRepository khachHangRepository)
		{
			_khachHangRepository = khachHangRepository;
		}

		public Task<KhachHang?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _khachHangRepository.GetByIdAsync(id, cancellationToken);
		}

		public Task<KhachHang?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
		{
			return _khachHangRepository.FindByEmailAsync(email, cancellationToken);
		}

		public async Task<(bool success, string? error)> UpdateProfileAsync(string customerId, UpdateCustomerProfileRequest request, CancellationToken cancellationToken = default)
		{
			var customer = await _khachHangRepository.GetByIdAsync(customerId, cancellationToken);
			if (customer == null) return (false, "Không tìm thấy khách hàng.");

			customer.HoTen = request.HoTen ?? customer.HoTen;
			customer.SoDienThoai = string.IsNullOrWhiteSpace(request.SoDienThoai) ? customer.SoDienThoai : request.SoDienThoai;
			customer.NgaySinh = request.NgaySinh ?? customer.NgaySinh;
			customer.GioiTinh = request.GioiTinh ?? customer.GioiTinh;
			customer.DiaChi = request.DiaChi ?? customer.DiaChi;
			customer.Image = request.Image ?? customer.Image;
			customer.ModifiedDate = DateTime.UtcNow;
			_khachHangRepository.Update(customer);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);
			return (true, null);
		}

		public async Task<bool> IsEmailVerifiedAsync(string customerId, CancellationToken cancellationToken = default)
		{
			var customer = await _khachHangRepository.GetByIdAsync(customerId, cancellationToken);
			return customer?.IsEmailVerified == true;
		}
	}
}

