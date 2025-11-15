using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Core.Interfaces.Options;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FPTDrink.Infrastructure.Services
{
	public class CustomerAuthService : ICustomerAuthService
	{
		private readonly IKhachHangRepository _khachHangRepository;
		private readonly IEmailService _emailService;
		private readonly ILogger<CustomerAuthService> _logger;
		private readonly JwtOptions _jwtOptions;

		private const int OtpLength = 6;
		private const int OtpExpiryMinutes = 10;
		private const int MaxOtpAttempts = 5;

		public CustomerAuthService(
			IKhachHangRepository khachHangRepository,
			IEmailService emailService,
			IOptions<JwtOptions> jwtOptions,
			ILogger<CustomerAuthService> logger)
		{
			_khachHangRepository = khachHangRepository;
			_emailService = emailService;
			_logger = logger;
			_jwtOptions = jwtOptions.Value;
		}

		public async Task<(bool success, string? error, KhachHang? customer)> RegisterAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default)
		{
			if (await _khachHangRepository.ExistsByUsernameAsync(request.TenDangNhap, cancellationToken))
			{
				return (false, "Tên đăng nhập đã tồn tại.", null);
			}
			if (await _khachHangRepository.ExistsByEmailAsync(request.Email, cancellationToken))
			{
				return (false, "Email đã được sử dụng.", null);
			}
			if (await _khachHangRepository.ExistsByPhoneAsync(request.SoDienThoai, cancellationToken))
			{
				return (false, "Số điện thoại đã được sử dụng.", null);
			}

			var entity = new KhachHang
			{
				Id = Guid.NewGuid().ToString("N"),
				TenDangNhap = request.TenDangNhap.Trim(),
				MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
				Email = request.Email.Trim(),
				SoDienThoai = request.SoDienThoai.Trim(),
				HoTen = request.HoTen.Trim(),
				IsActive = true,
				IsEmailVerified = false,
				EmailVerificationAttempts = 0,
				Status = 1,
				CreatedDate = DateTime.UtcNow,
				ModifiedDate = DateTime.UtcNow
			};

			SetOtpForCustomer(entity);
			await _khachHangRepository.AddAsync(entity, cancellationToken);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);
			await SendOtpEmailAsync(entity, cancellationToken);
			return (true, null, entity);
		}

		public async Task<(bool success, string? error)> SendVerificationOtpAsync(string email, CancellationToken cancellationToken = default)
		{
			var customer = await _khachHangRepository.FindByEmailAsync(email, cancellationToken);
			if (customer == null) return (false, "Không tìm thấy tài khoản.");
			if (customer.IsEmailVerified) return (false, "Tài khoản đã xác thực.");

			SetOtpForCustomer(customer);
			_khachHangRepository.Update(customer);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);
			await SendOtpEmailAsync(customer, cancellationToken);
			return (true, null);
		}

		public async Task<(bool success, string? error, KhachHang? customer, string? token)> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
		{
			var customer = await _khachHangRepository.FindByEmailAsync(request.Email, cancellationToken);
			if (customer == null) return (false, "Không tìm thấy tài khoản.", null, null);
			if (customer.IsEmailVerified)
			{
				return (true, null, customer, GenerateJwtToken(customer, true));
			}

			if (customer.EmailVerificationOtpExpiry < DateTime.UtcNow)
			{
				return (false, "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.", null, null);
			}

			if (!string.Equals(customer.EmailVerificationOtp, request.Otp, StringComparison.Ordinal))
			{
				customer.EmailVerificationAttempts += 1;
				if (customer.EmailVerificationAttempts >= MaxOtpAttempts)
				{
					SetOtpForCustomer(customer);
					await _khachHangRepository.SaveChangesAsync(cancellationToken);
					await SendOtpEmailAsync(customer, cancellationToken);
					return (false, "Bạn đã nhập sai quá số lần cho phép. Mã OTP mới đã được gửi.", null, null);
				}
				_khachHangRepository.Update(customer);
				await _khachHangRepository.SaveChangesAsync(cancellationToken);
				return (false, "Mã OTP không chính xác.", null, null);
			}

			customer.IsEmailVerified = true;
			customer.EmailVerificationOtp = null;
			customer.EmailVerificationOtpExpiry = null;
			customer.EmailVerificationAttempts = 0;
			customer.ModifiedDate = DateTime.UtcNow;
			_khachHangRepository.Update(customer);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);

			var token = GenerateJwtToken(customer, true);
			return (true, null, customer, token);
		}

		public async Task<(bool success, string? error, KhachHang? customer, string? token)> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
		{
			var byUsername = await _khachHangRepository.FindByUsernameAsync(usernameOrEmail, cancellationToken);
			var customer = byUsername ?? await _khachHangRepository.FindByEmailAsync(usernameOrEmail, cancellationToken);
			if (customer == null) return (false, "Tài khoản không tồn tại.", null, null);
			if (string.IsNullOrEmpty(customer.MatKhau) || !BCrypt.Net.BCrypt.Verify(password, customer.MatKhau))
			{
				return (false, "Mật khẩu không đúng.", null, null);
			}
			if (!customer.IsActive)
			{
				return (false, "Tài khoản đã bị khoá.", null, null);
			}
			if (!customer.IsEmailVerified)
			{
				return (false, "Tài khoản chưa được xác thực email.", null, null);
			}

			customer.LastLoginDate = DateTime.UtcNow;
			_khachHangRepository.Update(customer);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);

			var token = GenerateJwtToken(customer, true);
			return (true, null, customer, token);
		}

		public async Task<(bool success, string? error)> ChangePasswordAsync(string customerId, ChangeCustomerPasswordRequest request, CancellationToken cancellationToken = default)
		{
			var customer = await _khachHangRepository.GetByIdAsync(customerId, cancellationToken);
			if (customer == null) return (false, "Không tìm thấy tài khoản.");
			if (string.IsNullOrEmpty(customer.MatKhau) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, customer.MatKhau))
			{
				return (false, "Mật khẩu hiện tại không đúng.");
			}

			customer.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
			customer.ModifiedDate = DateTime.UtcNow;
			_khachHangRepository.Update(customer);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);
			return (true, null);
		}

		public string GenerateJwtToken(KhachHang customer, bool isVerified)
		{
			var handler = new JwtSecurityTokenHandler();
			var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(_jwtOptions.Key) ? "dev-key-change-in-production-please" : _jwtOptions.Key));
			var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
			var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes > 0 ? _jwtOptions.ExpiryMinutes : 60 * 24 * 7);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, customer.Id),
				new Claim(ClaimTypes.NameIdentifier, customer.Id),
				new Claim(ClaimTypes.Name, customer.HoTen ?? customer.TenDangNhap),
				new Claim(ClaimTypes.Email, customer.Email ?? string.Empty),
				new Claim(ClaimTypes.Role, "Customer"),
				new Claim("isVerified", isVerified ? "true" : "false"),
				new Claim("isActive", customer.IsActive ? "true" : "false")
			};

			var token = new JwtSecurityToken(
				issuer: string.IsNullOrWhiteSpace(_jwtOptions.Issuer) ? "FPTDrink" : _jwtOptions.Issuer,
				audience: string.IsNullOrWhiteSpace(_jwtOptions.Audience) ? "FPTDrinkClients" : _jwtOptions.Audience,
				claims: claims,
				expires: expires,
				signingCredentials: credentials);

			return handler.WriteToken(token);
		}

		public async Task<(bool success, string? error)> SendPasswordResetOtpAsync(string email, CancellationToken cancellationToken = default)
		{
			var customer = await _khachHangRepository.FindByEmailAsync(email, cancellationToken);
			if (customer == null)
			{
				return (false, "Email không tồn tại trong hệ thống.");
			}

			if (!customer.IsActive)
			{
				return (false, "Tài khoản đã bị khoá.");
			}

			customer.PasswordResetToken = GenerateOtp();
			customer.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
			_khachHangRepository.Update(customer);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);

			await SendPasswordResetOtpEmailAsync(customer, cancellationToken);
			return (true, null);
		}

		public async Task<(bool success, string? error)> ResetPasswordWithOtpAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
		{
			var customer = await _khachHangRepository.FindByEmailAsync(request.Email, cancellationToken);
			if (customer == null)
			{
				return (false, "Email không tồn tại trong hệ thống.");
			}

			if (string.IsNullOrWhiteSpace(customer.PasswordResetToken))
			{
				return (false, "Mã OTP không hợp lệ. Vui lòng yêu cầu mã mới.");
			}

			if (customer.PasswordResetExpiry < DateTime.UtcNow)
			{
				return (false, "Mã OTP đã hết hạn. Vui lòng yêu cầu mã mới.");
			}

			if (!string.Equals(customer.PasswordResetToken, request.Otp, StringComparison.Ordinal))
			{
				return (false, "Mã OTP không chính xác.");
			}

			// Reset password
			customer.MatKhau = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
			customer.PasswordResetToken = null;
			customer.PasswordResetExpiry = null;
			customer.ModifiedDate = DateTime.UtcNow;
			_khachHangRepository.Update(customer);
			await _khachHangRepository.SaveChangesAsync(cancellationToken);

			return (true, null);
		}

		private async Task SendPasswordResetOtpEmailAsync(KhachHang customer, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(customer.Email) || string.IsNullOrWhiteSpace(customer.PasswordResetToken))
			{
				return;
			}

			string body =
				$"""
				<!DOCTYPE html>
				<html lang="vi">
				<head>
					<meta charset="UTF-8">
					<title>Đặt lại mật khẩu</title>
				</head>
				<body style="font-family: Arial, sans-serif;">
					<p>Xin chào {customer.HoTen ?? customer.TenDangNhap},</p>
					<p>Bạn đã yêu cầu đặt lại mật khẩu. Mã OTP của bạn là:</p>
					<div style="font-size:32px;font-weight:bold;letter-spacing:6px;">{customer.PasswordResetToken}</div>
					<p>Mã có hiệu lực trong {OtpExpiryMinutes} phút.</p>
					<p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
					<p>Trân trọng,<br/>FPTDrink</p>
				</body>
				</html>
				""";

			try
			{
				await _emailService.SendAsync(customer.Email, "Đặt lại mật khẩu FPTDrink", body, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Gửi email OTP đặt lại mật khẩu thất bại cho {Email}", customer.Email);
			}
		}

		private void SetOtpForCustomer(KhachHang customer)
		{
			customer.EmailVerificationOtp = GenerateOtp();
			customer.EmailVerificationOtpExpiry = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes);
			customer.EmailVerificationAttempts = 0;
		}

		private static string GenerateOtp()
		{
			var random = new Random();
			var builder = new StringBuilder(OtpLength);
			for (int i = 0; i < OtpLength; i++)
			{
				builder.Append(random.Next(0, 10));
			}
			return builder.ToString();
		}

		private async Task SendOtpEmailAsync(KhachHang customer, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(customer.Email) || string.IsNullOrWhiteSpace(customer.EmailVerificationOtp))
			{
				return;
			}

			string body =
				$"""
				<!DOCTYPE html>
				<html lang="vi">
				<head>
					<meta charset="UTF-8">
					<title>Xác thực tài khoản</title>
				</head>
				<body style="font-family: Arial, sans-serif;">
					<p>Xin chào {customer.HoTen ?? customer.TenDangNhap},</p>
					<p>Mã OTP của bạn là:</p>
					<div style="font-size:32px;font-weight:bold;letter-spacing:6px;">{customer.EmailVerificationOtp}</div>
					<p>Mã có hiệu lực trong {OtpExpiryMinutes} phút.</p>
					<p>Nếu bạn không yêu cầu, vui lòng bỏ qua email này.</p>
					<p>Trân trọng,<br/>FPTDrink</p>
				</body>
				</html>
				""";

			try
			{
				await _emailService.SendAsync(customer.Email, "Xác thực tài khoản FPTDrink", body, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Gửi email OTP thất bại cho {Email}", customer.Email);
			}
		}

	}
}
