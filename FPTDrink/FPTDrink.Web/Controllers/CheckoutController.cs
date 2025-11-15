using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using FPTDrink.Web.Extensions;
using FPTDrink.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace FPTDrink.Web.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly ApiClient _api;
		private readonly IHttpClientFactory _httpClientFactory;
		private const string CartCookie = "cart_id";

		public CheckoutController(ApiClient api, IHttpClientFactory httpClientFactory)
		{
			_api = api;
			_httpClientFactory = httpClientFactory;
		}

		private string? GetCartId() => Request.Cookies.TryGetValue(CartCookie, out var v) ? v : null;

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var cartId = GetCartId();
			var cart = await _api.GetAsync<CartViewModel>($"api/public/Cart?cartId={cartId}") ?? new CartViewModel();
			if (cart.Items.Count == 0)
			{
				return RedirectToAction("Index", "ShoppingCart");
			}
			ViewBag.Cart = cart;

			var model = new CheckoutViewModel();

			// Nếu customer đã đăng nhập, lấy thông tin và pre-fill form
			if (HttpContext.IsCustomerAuthenticated())
			{
				var customerId = HttpContext.GetCustomerId();
				if (!string.IsNullOrWhiteSpace(customerId))
				{
					try
					{
						// Lấy token từ cookie để gọi API
						var token = Request.Cookies["customer_token"];
						if (!string.IsNullOrWhiteSpace(token))
						{
							// Tạo HttpClient với token để gọi API customer profile
							using var httpClient = new System.Net.Http.HttpClient();
							httpClient.DefaultRequestHeaders.Authorization = 
								new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
							
							var apiBaseUrl = HttpContext.RequestServices
								.GetRequiredService<IConfiguration>()["ApiBaseUrl"] ?? "http://localhost:5213";
							httpClient.BaseAddress = new Uri(apiBaseUrl);

							var response = await httpClient.GetAsync("api/public/customer/profile");
							if (response.IsSuccessStatusCode)
							{
								var customerProfile = await response.Content.ReadFromJsonAsync<CustomerProfileDto>(
									new JsonSerializerOptions 
									{ 
										PropertyNameCaseInsensitive = true,
										PropertyNamingPolicy = JsonNamingPolicy.CamelCase
									});
								if (customerProfile != null)
								{
									model.TenKhachHang = customerProfile.HoTen ?? string.Empty;
									model.SoDienThoai = customerProfile.SoDienThoai ?? string.Empty;
									model.Email = customerProfile.Email ?? string.Empty;
									model.DiaChi = customerProfile.DiaChi ?? string.Empty;
								}
							}
						}
					}
					catch (Exception ex)
					{
						// Log lỗi nhưng không block checkout flow
						var logger = HttpContext.RequestServices.GetRequiredService<ILogger<CheckoutController>>();
						logger.LogWarning(ex, "Không thể lấy thông tin customer để pre-fill form");
					}
				}
			}

			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Index(CheckoutViewModel model)
		{
			var cartId = GetCartId();
			var cart = await _api.GetAsync<CartViewModel>($"api/public/Cart?cartId={cartId}") ?? new CartViewModel();
			if (!ModelState.IsValid || cart.Items.Count == 0)
			{
				ViewBag.Cart = cart;
				return View(model);
			}

			model.Items = cart.Items.Select(i => new CheckoutViewModel.CartItem { ProductId = i.ProductId, Quantity = i.Quantity }).ToList();

			var createRes = await _api.PostAsync("api/public/Checkout/order", new
			{
				model.TenKhachHang,
				model.SoDienThoai,
				model.DiaChi,
				model.Email,
				model.TypePayment,
				Items = model.Items
			});

			if (!createRes.IsSuccessStatusCode)
			{
				ModelState.AddModelError(string.Empty, "Tạo đơn hàng thất bại. Vui lòng thử lại.");
				ViewBag.Cart = cart;
				return View(model);
			}

			var orderCreated = await createRes.Content.ReadFromJsonAsync<CreateOrderResponse>();
			var orderCode = orderCreated?.OrderCode ?? "";

			if (orderCreated?.Order != null)
			{
				TempData["LastOrderDetail"] = JsonSerializer.Serialize(orderCreated.Order);
			}

			if (!string.IsNullOrWhiteSpace(cartId))
			{
				try
				{
					await _api.PostAsync<object>($"api/public/Cart/clear?cartId={Uri.EscapeDataString(cartId)}", new { });
				}
				catch
				{
				}
			}

			if (model.TypePayment == 2)
			{
				var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
				var vnpay = await _api.PostAsync("api/public/Checkout/payment/vnpay", new
				{
					OrderCode = orderCode,
					TypePaymentVN = 0,
					ReturnUrlOverride = Url.ActionLink("VnpayReturn", "Checkout", values: null, protocol: Request.Scheme),
					ClientIp = remoteIp
				});
				if (vnpay.IsSuccessStatusCode)
				{
					var init = await vnpay.Content.ReadFromJsonAsync<VnPayInitResponse>();
					if (!string.IsNullOrWhiteSpace(init?.PaymentUrl))
					{
						return Redirect(init.PaymentUrl);
					}
				}
				TempData["PaymentMessage"] = "Không thể chuyển đến cổng thanh toán VNPay. Vui lòng thử lại hoặc chọn phương thức khác.";
				ViewBag.Cart = cart;
				return View(model);
			}

			// Redirect to success page for non-VNPay payments (in-store or COD)
			return RedirectToAction("Success", new { orderCode = orderCode });
		}

		[HttpGet]
		public async Task<IActionResult> Success(string orderCode)
		{
			ViewBag.OrderCode = orderCode;
			if (string.IsNullOrWhiteSpace(orderCode))
			{
				return View(model: null);
			}

			if (TempData.TryGetValue("LastOrderDetail", out var cachedObj) && cachedObj is string cachedJson && !string.IsNullOrWhiteSpace(cachedJson))
			{
				try
				{
					var cachedOrder = JsonSerializer.Deserialize<OrderDetailViewModel>(cachedJson);
					if (cachedOrder != null && string.Equals(cachedOrder.MaHoaDon, orderCode, StringComparison.OrdinalIgnoreCase))
					{
						return View(cachedOrder);
					}
				}
				catch
				{
				}
			}

			var order = await GetOrderByCodeAsync(orderCode);
			return View(order);
		}

		[HttpPost]
		public async Task<IActionResult> SendInvoice(string orderCode)
		{
			if (string.IsNullOrWhiteSpace(orderCode))
			{
				return Json(new { success = false, message = "Mã đơn hàng không hợp lệ." });
			}

			var response = await _api.PostAsync<object>($"api/public/Checkout/order/{Uri.EscapeDataString(orderCode)}/send-email", new { });
			if (response.IsSuccessStatusCode)
			{
				return Json(new { success = true, message = "Hoá đơn đã được gửi về email của bạn." });
			}

			var error = await response.Content.ReadAsStringAsync();
			return Json(new { success = false, message = string.IsNullOrWhiteSpace(error) ? "Gửi hoá đơn thất bại. Vui lòng thử lại." : error });
		}

		[HttpGet]
		public IActionResult VnpayReturn(string? vnp_TxnRef, string? vnp_ResponseCode, string? vnp_TransactionStatus, string? vnp_ResponseCodeDesc)
		{
			if (string.Equals(vnp_ResponseCode, "00", StringComparison.OrdinalIgnoreCase))
			{
				return RedirectToAction("Success", new { orderCode = vnp_TxnRef });
			}

			TempData["PaymentErrorMessage"] = string.IsNullOrWhiteSpace(vnp_ResponseCodeDesc)
				? "Thanh toán không thành công hoặc đã bị hủy."
				: vnp_ResponseCodeDesc;
			return RedirectToAction("Failure", new { orderCode = vnp_TxnRef });
		}

		[HttpGet]
		public async Task<IActionResult> Failure(string orderCode)
		{
			var message = TempData["PaymentErrorMessage"] as string ?? "Thanh toán không thành công hoặc đã bị hủy.";
			OrderDetailViewModel? order = null;
			if (!string.IsNullOrWhiteSpace(orderCode))
			{
				order = await GetOrderByCodeAsync(orderCode);
			}

			ViewBag.Message = message;
			return View(order);
		}

		private async Task<OrderDetailViewModel?> GetOrderByCodeAsync(string orderCode)
		{
			var httpClient = _httpClientFactory.CreateClient("FPTDrinkApi");
			var token = HttpContext.Request.Cookies["customer_token"];
			if (!string.IsNullOrWhiteSpace(token))
			{
				httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
			}

			var response = await httpClient.GetAsync($"api/public/Orders/{System.Net.WebUtility.UrlEncode(orderCode)}");
			if (!response.IsSuccessStatusCode) return null;

			var jsonContent = await response.Content.ReadAsStringAsync();
			var jsonDoc = JsonDocument.Parse(jsonContent);
			return OrderMappingHelper.MapFromJson(jsonDoc.RootElement);
		}
	}
}

