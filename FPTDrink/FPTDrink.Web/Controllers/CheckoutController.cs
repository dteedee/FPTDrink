using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
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
				model.CCCD,
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

			if (model.TypePayment == 2)
			{
				// VNPay
				var vnpay = await _api.PostAsync("api/public/Checkout/payment/vnpay", new
				{
					OrderCode = orderCode,
					TypePaymentVN = 2,
					ReturnUrlOverride = Url.ActionLink("VnpayReturn", "Checkout", values: null, protocol: Request.Scheme)
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

			// COD / fallback
			return RedirectToAction("Success", new { orderCode });
		}

		[HttpGet]
		public IActionResult Success(string orderCode)
		{
			ViewBag.OrderCode = orderCode;
			return View();
		}

		[HttpGet]
		public IActionResult VnpayReturn(string? vnp_TxnRef, string? vnp_ResponseCode, string? vnp_TransactionStatus)
		{
			// Dẫn tới trang success và client sẽ tra cứu đơn mã
			return RedirectToAction("Success", new { orderCode = vnp_TxnRef });
		}
	}
}

