using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Web.Areas.Admin.ViewModels;
using FPTDrink.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class AuthController : Controller
	{
		private readonly IAuthService _authService;
		private readonly IChucVuRepository _roleRepository;
		private readonly IConfiguration _configuration;

		public AuthController(IAuthService authService, IChucVuRepository roleRepository, IConfiguration configuration)
		{
			_authService = authService;
			_roleRepository = roleRepository;
			_configuration = configuration;
		}

		[HttpGet]
		public IActionResult Login(string? returnUrl = null)
		{
			if (!string.IsNullOrWhiteSpace(HttpContext.Session.GetString(AdminSessionKeys.UserId)))
			{
				return RedirectToLocal(returnUrl);
			}
			ViewBag.ReturnUrl = returnUrl;
			ViewBag.SupportEmail = _configuration["Email:Admin"] ?? _configuration["Email:Sender"];
			return View(new AdminLoginViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.ReturnUrl = returnUrl;
				ViewBag.SupportEmail = _configuration["Email:Admin"] ?? _configuration["Email:Sender"];
				return View(model);
			}

			var (success, error, user) = await _authService.LoginAsync(model.Username, model.Password, cancellationToken);
			if (!success || user == null)
			{
				ModelState.AddModelError(string.Empty, error ?? "Đăng nhập thất bại. Vui lòng thử lại.");
				ViewBag.ReturnUrl = returnUrl;
				ViewBag.SupportEmail = _configuration["Email:Admin"] ?? _configuration["Email:Sender"];
				return View(model);
			}

			string? roleName = "Chưa phân quyền";
			if (user.IdChucVu.HasValue)
			{
				var role = await _roleRepository.Query().FirstOrDefaultAsync(r => r.Id == user.IdChucVu.Value, cancellationToken);
				if (role != null && !string.IsNullOrWhiteSpace(role.TenChucVu))
				{
					roleName = role.TenChucVu;
				}
			}

			HttpContext.Session.SetString(AdminSessionKeys.UserId, user.Id);
			HttpContext.Session.SetString(AdminSessionKeys.Username, user.TenDangNhap);
			HttpContext.Session.SetString(AdminSessionKeys.DisplayName, user.TenHienThi ?? user.FullName);
			HttpContext.Session.SetString(AdminSessionKeys.FullName, user.FullName ?? string.Empty);
			HttpContext.Session.SetString(AdminSessionKeys.RoleName, roleName);

			return RedirectToLocal(returnUrl);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Logout()
		{
			HttpContext.Session.Remove(AdminSessionKeys.UserId);
			HttpContext.Session.Remove(AdminSessionKeys.Username);
			HttpContext.Session.Remove(AdminSessionKeys.DisplayName);
			HttpContext.Session.Remove(AdminSessionKeys.FullName);
			HttpContext.Session.Remove(AdminSessionKeys.RoleName);
			HttpContext.Session.Clear();
			return RedirectToAction(nameof(Login));
		}

		private IActionResult RedirectToLocal(string? returnUrl)
		{
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
		}
	}
}

