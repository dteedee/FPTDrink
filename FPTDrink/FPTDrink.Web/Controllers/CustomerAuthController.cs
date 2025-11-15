using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using FPTDrink.Web.Extensions;
using FPTDrink.Web.Constants;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

namespace FPTDrink.Web.Controllers
{
	public class CustomerAuthController : Controller
	{
		private readonly ApiClient _apiClient;
		private readonly ILogger<CustomerAuthController> _logger;
		private readonly IWebHostEnvironment _environment;
		private readonly IAuthService _authService;
		private readonly IChucVuRepository _chucVuRepository;
		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true
		};

		public CustomerAuthController(
			ApiClient apiClient, 
			ILogger<CustomerAuthController> logger, 
			IWebHostEnvironment environment,
			IAuthService authService,
			IChucVuRepository chucVuRepository)
		{
			_apiClient = apiClient;
			_logger = logger;
			_environment = environment;
			_authService = authService;
			_chucVuRepository = chucVuRepository;
		}

		private CookieOptions GetCookieOptions(DateTimeOffset? expires = null)
		{
			return new CookieOptions
			{
				HttpOnly = true,
				Secure = !_environment.IsDevelopment(), // Chỉ dùng Secure trong Production
				SameSite = SameSiteMode.Strict,
				Expires = expires ?? DateTimeOffset.UtcNow.AddDays(30)
			};
		}

		[HttpGet]
		public IActionResult Register()
		{
			// Nếu đã đăng nhập, redirect về trang chủ
			if (HttpContext.IsCustomerAuthenticated())
			{
				return RedirectToAction("Index", "Home");
			}
			return View(new CustomerRegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(CustomerRegisterViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var registerDto = new
				{
					tenDangNhap = model.TenDangNhap,
					matKhau = model.MatKhau,
					email = model.Email,
					soDienThoai = model.SoDienThoai,
					hoTen = model.HoTen
				};

				var response = await _apiClient.PostAsync("api/public/customer-auth/register", registerDto, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);
					var message = result.GetProperty("message").GetString() ?? "Đăng ký thành công";
					TempData["SuccessMessage"] = message;
					return RedirectToAction(nameof(VerifyOtp), new { email = model.Email });
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
					var errorMessage = errorJson.TryGetProperty("message", out var msgProp) 
						? msgProp.GetString() 
						: "Đăng ký thất bại. Vui lòng thử lại.";
					ModelState.AddModelError(string.Empty, errorMessage ?? "Đăng ký thất bại. Vui lòng thử lại.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi đăng ký tài khoản");
				ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại sau.");
			}

			return View(model);
		}

		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			// Nếu đã đăng nhập, redirect về trang chủ hoặc returnUrl
			if (HttpContext.IsCustomerAuthenticated())
			{
				return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
			}
			ViewBag.ReturnUrl = returnUrl;
			return View(new CustomerLoginViewModel { ReturnUrl = returnUrl });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(CustomerLoginViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				// Bước 1: Thử đăng nhập với Admin/Employee trước
				var (adminSuccess, adminError, adminUser) = await _authService.LoginAsync(model.UsernameOrEmail, model.Password, cancellationToken);
				
				if (adminSuccess && adminUser != null)
				{
					// Lấy thông tin chức vụ
					string? roleName = "Chưa phân quyền";
					if (adminUser.IdChucVu.HasValue)
					{
						var role = await _chucVuRepository.Query()
							.FirstOrDefaultAsync(r => r.Id == adminUser.IdChucVu.Value, cancellationToken);
						if (role != null && !string.IsNullOrWhiteSpace(role.TenChucVu))
						{
							roleName = role.TenChucVu;
						}
					}

					// Lưu thông tin vào session
					HttpContext.Session.SetString(AdminSessionKeys.UserId, adminUser.Id);
					HttpContext.Session.SetString(AdminSessionKeys.Username, adminUser.TenDangNhap);
					HttpContext.Session.SetString(AdminSessionKeys.DisplayName, adminUser.TenHienThi ?? adminUser.FullName);
					HttpContext.Session.SetString(AdminSessionKeys.FullName, adminUser.FullName ?? string.Empty);
					HttpContext.Session.SetString(AdminSessionKeys.RoleName, roleName);

					// Redirect theo role
					var redirectUrl = GetRedirectUrlByRole(roleName, model.ReturnUrl);
					_logger.LogInformation("Admin login successful. Role: {RoleName}, Redirect URL: {RedirectUrl}", roleName, redirectUrl);
					TempData["SuccessMessage"] = "Đăng nhập thành công!";
					return Redirect(redirectUrl);
				}

				// Bước 2: Nếu không phải admin/employee, thử đăng nhập với Customer
				var loginDto = new
				{
					usernameOrEmail = model.UsernameOrEmail,
					password = model.Password
				};

				var response = await _apiClient.PostAsync("api/public/customer-auth/login", loginDto, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);
					var token = result.GetProperty("token").GetString();
					var customer = result.GetProperty("customer");

					if (!string.IsNullOrEmpty(token))
					{
						// Lưu token vào cookie
						var expires = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(24);
						Response.Cookies.Append("customer_token", token, GetCookieOptions(expires));

						TempData["SuccessMessage"] = "Đăng nhập thành công!";
						return Redirect(string.IsNullOrEmpty(model.ReturnUrl) ? "/" : model.ReturnUrl);
					}
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
					var errorMessage = errorJson.TryGetProperty("message", out var msgProp)
						? msgProp.GetString()
						: "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
					ModelState.AddModelError(string.Empty, errorMessage ?? "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi đăng nhập");
				ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại sau.");
			}

			return View(model);
		}

		private string GetRedirectUrlByRole(string roleName, string? returnUrl)
		{
			// Nếu có returnUrl và là local URL, ưu tiên returnUrl
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return returnUrl;
			}

			// Normalize role name for comparison
			var normalizedRole = roleName?.Trim().ToLower() ?? string.Empty;

			// Redirect theo role - tất cả admin/employee đều redirect về admin dashboard
			// Kiểm tra nếu là admin/employee role
			if (normalizedRole.Contains("admin") || 
			    normalizedRole.Contains("quản trị") || 
			    normalizedRole.Contains("quản lý") ||
			    normalizedRole.Contains("thu ngân") || 
			    normalizedRole.Contains("thungan") ||
			    normalizedRole.Contains("kế toán") || 
			    normalizedRole.Contains("ketoan") ||
			    string.IsNullOrWhiteSpace(normalizedRole) ||
			    normalizedRole == "chưa phân quyền")
			{
				// Sử dụng đường dẫn tuyệt đối để đảm bảo redirect đúng
				return "/admin/Dashboard";
			}

			// Mặc định redirect về admin dashboard
			return "/admin/Dashboard";
		}

		[HttpGet]
		public IActionResult VerifyOtp(string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return RedirectToAction(nameof(Register));
			}

			return View(new VerifyOtpViewModel { Email = email });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var verifyDto = new
				{
					email = model.Email,
					otp = model.Otp
				};

				var response = await _apiClient.PostAsync("api/public/customer-auth/verify-otp", verifyDto, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions, cancellationToken);
					var token = result.GetProperty("token").GetString();

					if (!string.IsNullOrEmpty(token))
					{
						Response.Cookies.Append("customer_token", token, GetCookieOptions());

						TempData["SuccessMessage"] = "Xác thực email thành công! Tài khoản của bạn đã được kích hoạt.";
						return RedirectToAction("Index", "Home");
					}
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
					var errorMessage = errorJson.TryGetProperty("message", out var msgProp)
						? msgProp.GetString()
						: "Mã OTP không đúng. Vui lòng thử lại.";
					ModelState.AddModelError(string.Empty, errorMessage ?? "Mã OTP không đúng. Vui lòng thử lại.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi xác thực OTP");
				ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại sau.");
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResendOtp(string email, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return BadRequest("Email không hợp lệ");
			}

			try
			{
				var resendDto = new { email };
				var response = await _apiClient.PostAsync("api/public/customer-auth/resend-otp", resendDto, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					TempData["SuccessMessage"] = "Mã OTP mới đã được gửi đến email của bạn.";
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
					var errorMessage = errorJson.TryGetProperty("message", out var msgProp)
						? msgProp.GetString()
						: "Không thể gửi lại mã OTP. Vui lòng thử lại.";
					TempData["ErrorMessage"] = errorMessage;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi gửi lại OTP");
				TempData["ErrorMessage"] = "Đã xảy ra lỗi. Vui lòng thử lại sau.";
			}

			return RedirectToAction(nameof(VerifyOtp), new { email });
		}


		[HttpGet]
		public IActionResult ForgotPassword()
		{
			return View(new ForgotPasswordViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var forgotPasswordDto = new { email = model.Email };
				var response = await _apiClient.PostAsync("api/public/customer-auth/forgot-password", forgotPasswordDto, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					TempData["SuccessMessage"] = "Mã OTP đã được gửi đến email của bạn. Vui lòng kiểm tra email.";
					return RedirectToAction(nameof(ResetPassword), new { email = model.Email });
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
					var errorMessage = errorJson.TryGetProperty("message", out var msgProp)
						? msgProp.GetString()
						: "Không thể gửi mã OTP. Vui lòng thử lại.";
					ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể gửi mã OTP. Vui lòng thử lại.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi gửi OTP quên mật khẩu");
				ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại sau.");
			}

			return View(model);
		}

		[HttpGet]
		public IActionResult ResetPassword(string email)
		{
			return View(new ResetPasswordViewModel { Email = email ?? string.Empty });
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var resetPasswordDto = new
				{
					email = model.Email,
					otp = model.Otp,
					newPassword = model.NewPassword,
					confirmNewPassword = model.ConfirmNewPassword
				};

				var response = await _apiClient.PostAsync("api/public/customer-auth/reset-password", resetPasswordDto, cancellationToken);

				if (response.IsSuccessStatusCode)
				{
					TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập với mật khẩu mới.";
					return RedirectToAction(nameof(Login));
				}
				else
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
					var errorMessage = errorJson.TryGetProperty("message", out var msgProp)
						? msgProp.GetString()
						: "Đặt lại mật khẩu thất bại. Vui lòng thử lại.";
					ModelState.AddModelError(string.Empty, errorMessage ?? "Đặt lại mật khẩu thất bại. Vui lòng thử lại.");
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Lỗi khi đặt lại mật khẩu");
				ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi. Vui lòng thử lại sau.");
			}

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
			Response.Cookies.Delete("customer_token");
			TempData["SuccessMessage"] = "Đăng xuất thành công!";
			return RedirectToAction("Index", "Home");
		}
	}
}

