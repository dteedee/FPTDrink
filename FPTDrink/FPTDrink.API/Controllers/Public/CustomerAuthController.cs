using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FPTDrink.API.DTOs.Public.CustomerAuth;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/customer-auth")]
	public class CustomerAuthController : ControllerBase
	{
		private readonly ICustomerAuthService _customerAuthService;
		private readonly IMapper _mapper;

		public CustomerAuthController(ICustomerAuthService customerAuthService, IMapper mapper)
		{
			_customerAuthService = customerAuthService;
			_mapper = mapper;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var (success, error, customer) = await _customerAuthService.RegisterAsync(
				new CustomerRegisterRequest(dto.TenDangNhap, dto.MatKhau, dto.Email, dto.SoDienThoai, dto.HoTen), ct);
			if (!success) return BadRequest(new { message = error });
			return Ok(new { message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản." });
		}

		[HttpPost("resend-otp")]
		public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var (success, error) = await _customerAuthService.SendVerificationOtpAsync(dto.Email, ct);
			if (!success) return BadRequest(new { message = error });
			return Ok(new { message = "OTP mới đã được gửi." });
		}

		[HttpPost("verify-otp")]
		public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var (success, error, customer, token) = await _customerAuthService.VerifyOtpAsync(
				new VerifyOtpRequest(dto.Email, dto.Otp), ct);
			if (!success || customer == null || token == null) return BadRequest(new { message = error });
			var response = new CustomerAuthResponseDto
			{
				Token = token,
				Customer = _mapper.Map<CustomerProfileDto>(customer)
			};
			return Ok(response);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var (success, error, customer, token) = await _customerAuthService.LoginAsync(dto.UsernameOrEmail, dto.Password, ct);
			if (!success || customer == null || token == null) return BadRequest(new { message = error });
			return Ok(new CustomerAuthResponseDto
			{
				Token = token,
				Customer = _mapper.Map<CustomerProfileDto>(customer)
			});
		}

		[HttpPost("change-password")]
		[Authorize(Policy = "Customer")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangeCustomerPasswordRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
			if (string.IsNullOrWhiteSpace(customerId)) return Forbid();
			var (success, error) = await _customerAuthService.ChangePasswordAsync(customerId,
				new ChangeCustomerPasswordRequest(dto.CurrentPassword, dto.NewPassword), ct);
			if (!success) return BadRequest(new { message = error });
			return Ok(new { message = "Đổi mật khẩu thành công." });
		}

		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ResendOtpRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var (success, error) = await _customerAuthService.SendPasswordResetOtpAsync(dto.Email, ct);
			if (!success) return BadRequest(new { message = error });
			return Ok(new { message = "Mã OTP đã được gửi đến email của bạn." });
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var (success, error) = await _customerAuthService.ResetPasswordWithOtpAsync(
				new ResetPasswordRequest(dto.Email, dto.Otp, dto.NewPassword), ct);
			if (!success) return BadRequest(new { message = error });
			return Ok(new { message = "Đặt lại mật khẩu thành công." });
		}
	}
}

