using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;

namespace FPTDrink.Infrastructure.Services
{
	public class AuthService : IAuthService
	{
		private readonly INhanVienRepository _nhanVienRepository;

		public AuthService(INhanVienRepository nhanVienRepository)
		{
			_nhanVienRepository = nhanVienRepository;
		}

		public async Task<(bool success, string? error, NhanVien? user)> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				return (false, "Tên đăng nhập hoặc mật khẩu không được để trống!", null);
			}

			var user = await _nhanVienRepository.FindByUsernameAsync(username, cancellationToken);
			if (user == null)
			{
				return (false, "Tài khoản này chưa được tạo!", null);
			}
			if (!string.Equals(user.TenDangNhap, username, StringComparison.Ordinal))
			{
				return (false, "Tên đăng nhập không đúng!", null);
			}
			if (!string.Equals(user.MatKhau, password, StringComparison.Ordinal))
			{
				return (false, "Mật khẩu đăng nhập không đúng!", null);
			}
			if (user.IsActiveAccount == false)
			{
				return (false, "Tài khoản đã bị khoá!", null);
			}
			return (true, null, user);
		}

		public Task LogoutAsync()
		{
			return Task.CompletedTask;
		}
	}
}


