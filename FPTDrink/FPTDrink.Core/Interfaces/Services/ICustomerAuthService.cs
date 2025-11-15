using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface ICustomerAuthService
	{
		Task<(bool success, string? error, KhachHang? customer)> RegisterAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> SendVerificationOtpAsync(string email, CancellationToken cancellationToken = default);
		Task<(bool success, string? error, KhachHang? customer, string? token)> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default);
		Task<(bool success, string? error, KhachHang? customer, string? token)> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> ChangePasswordAsync(string customerId, ChangeCustomerPasswordRequest request, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> SendPasswordResetOtpAsync(string email, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> ResetPasswordWithOtpAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
		string GenerateJwtToken(KhachHang customer, bool isVerified);
	}

	public record CustomerRegisterRequest(
		string TenDangNhap,
		string MatKhau,
		string Email,
		string SoDienThoai,
		string HoTen);

	public record VerifyOtpRequest(string Email, string Otp);

	public record ChangeCustomerPasswordRequest(
		string CurrentPassword,
		string NewPassword);

	public record ResetPasswordRequest(
		string Email,
		string Otp,
		string NewPassword);
}

