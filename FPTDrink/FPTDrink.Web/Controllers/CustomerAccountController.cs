using System.Threading;
using System.Threading.Tasks;
using FPTDrink.Web.Extensions;
using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FPTDrink.Web.Controllers
{
	public class CustomerAccountController : Controller
	{
		private readonly ApiClient _apiClient;
		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true
		};

		public CustomerAccountController(ApiClient apiClient)
		{
			_apiClient = apiClient;
		}

		[HttpGet]
		public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
		{
			if (!HttpContext.IsCustomerAuthenticated())
			{
				return RedirectToAction("Login", "CustomerAuth", new { returnUrl = "/CustomerAccount" });
			}

			var customerId = HttpContext.GetCustomerId();
			if (string.IsNullOrWhiteSpace(customerId))
			{
				return RedirectToAction("Login", "CustomerAuth");
			}

			var customer = await _apiClient.GetAsync<CustomerProfileDto>("api/public/customer/profile", cancellationToken);
			if (customer == null)
			{
				TempData["ErrorMessage"] = "Không thể tải thông tin tài khoản.";
				return RedirectToAction("Index", "Home");
			}

			var model = new CustomerAccountViewModel
			{
				Profile = customer
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel updateModel, CancellationToken cancellationToken = default)
		{
			if (!HttpContext.IsCustomerAuthenticated())
			{
				return RedirectToAction("Login", "CustomerAuth");
			}

			// Validate ngày sinh
			if (updateModel.NgaySinh.HasValue)
			{
				var today = DateTime.Today;
				var maxDate = today.AddYears(-15); // Phải trên 15 tuổi

				if (updateModel.NgaySinh.Value > today)
				{
					ModelState.AddModelError(nameof(updateModel.NgaySinh), "Ngày sinh không được ở tương lai");
				}
				else if (updateModel.NgaySinh.Value > maxDate)
				{
					ModelState.AddModelError(nameof(updateModel.NgaySinh), "Bạn phải trên 15 tuổi để sử dụng dịch vụ");
				}
			}

			if (!ModelState.IsValid)
			{
				// Reload profile data if validation fails
				var customer = await _apiClient.GetAsync<CustomerProfileDto>("api/public/customer/profile", cancellationToken);
				var model = new CustomerAccountViewModel
				{
					Profile = customer ?? new CustomerProfileDto()
				};
				// Update với dữ liệu từ form
				if (customer != null)
				{
					model.Profile.HoTen = updateModel.HoTen;
					model.Profile.SoDienThoai = updateModel.SoDienThoai;
					model.Profile.NgaySinh = updateModel.NgaySinh;
					model.Profile.GioiTinh = updateModel.GioiTinh;
					model.Profile.DiaChi = updateModel.DiaChi;
				}
				return View("Index", model);
			}

			var updateDto = new
			{
				hoTen = updateModel.HoTen,
				soDienThoai = updateModel.SoDienThoai,
				ngaySinh = updateModel.NgaySinh,
				gioiTinh = updateModel.GioiTinh,
				diaChi = updateModel.DiaChi,
				image = (string?)null
			};

			var response = await _apiClient.PutAsync("api/public/customer/profile", updateDto, cancellationToken);
			if (response.IsSuccessStatusCode)
			{
				TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
			}
			else
			{
				var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
				var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
				var errorMessage = errorJson.TryGetProperty("message", out var msgProp)
					? msgProp.GetString()
					: "Cập nhật thông tin thất bại.";
				TempData["ErrorMessage"] = errorMessage;
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken cancellationToken = default)
		{
			if (!HttpContext.IsCustomerAuthenticated())
			{
				return RedirectToAction("Login", "CustomerAuth");
			}

			if (!ModelState.IsValid)
			{
				var customer = await _apiClient.GetAsync<CustomerProfileDto>("api/public/customer/profile", cancellationToken);
				var accountModel = new CustomerAccountViewModel { Profile = customer ?? new CustomerProfileDto() };
				ViewBag.ChangePasswordModel = model;
				return View("Index", accountModel);
			}

			var changePasswordDto = new
			{
				currentPassword = model.CurrentPassword,
				newPassword = model.NewPassword,
				confirmNewPassword = model.ConfirmNewPassword
			};

			var response = await _apiClient.PostAsync("api/public/customer-auth/change-password", changePasswordDto, cancellationToken);
			if (response.IsSuccessStatusCode)
			{
				TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
			}
			else
			{
				var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
				var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent, JsonOptions);
				var errorMessage = errorJson.TryGetProperty("message", out var msgProp)
					? msgProp.GetString()
					: "Đổi mật khẩu thất bại.";
				TempData["ErrorMessage"] = errorMessage;
			}

			return RedirectToAction(nameof(Index));
		}
	}
}

