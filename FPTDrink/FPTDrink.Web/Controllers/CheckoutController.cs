using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace FPTDrink.Web.Controllers
{
	public class CheckoutController : Controller
	{
		private readonly ApiClient _api;
		private const string CartCookie = "cart_id";

		public CheckoutController(ApiClient api)
		{
			_api = api;
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
			return View(new CheckoutViewModel());
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
					TypePaymentVN = model.TypePaymentVN,
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
			}

			return RedirectToAction("Success", new { orderCode });
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

			var order = await _api.GetAsync<OrderDetailViewModel>($"api/public/Orders/{orderCode}");
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
		public IActionResult VnpayReturn(string? vnp_TxnRef, string? vnp_ResponseCode, string? vnp_TransactionStatus)
		{
			return RedirectToAction("Success", new { orderCode = vnp_TxnRef });
		}
	}
}

