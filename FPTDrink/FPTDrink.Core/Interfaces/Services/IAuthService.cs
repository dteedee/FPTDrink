using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface IAuthService
	{
		Task<(bool success, string? error, NhanVien? user)> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
		Task LogoutAsync();
	}
}


