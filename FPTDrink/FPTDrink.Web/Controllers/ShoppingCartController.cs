using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FPTDrink.Web.Controllers
{
	public class ShoppingCartController : Controller
	{
		private readonly ApiClient _api;
		private const string CartCookie = "cart_id";

		public ShoppingCartController(ApiClient api)
		{
			_api = api;
		}

		private string EnsureCartId()
		{
			if (Request.Cookies.TryGetValue(CartCookie, out var existing) && !string.IsNullOrWhiteSpace(existing))
			{
				return existing;
			}
			var id = Guid.NewGuid().ToString("N");
			Response.Cookies.Append(CartCookie, id, new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30), HttpOnly = true, IsEssential = true });
			return id;
		}

		public async Task<IActionResult> Index()
		{
			var cartId = EnsureCartId();
			var cart = await _api.GetAsync<CartViewModel>($"api/public/Cart?cartId={cartId}") ?? new CartViewModel { CartId = cartId };
			return View(cart);
		}

		[HttpPost]
		public async Task<IActionResult> Add(string productId, int quantity = 1, string? returnUrl = null)
		{
			try
			{
				var cartId = EnsureCartId();
				await _api.PostAsync("api/public/Cart/items", new { ProductId = productId, Quantity = Math.Max(1, quantity), CartId = cartId });
				TempData["SuccessMessage"] = $"Đã thêm {quantity} sản phẩm vào giỏ hàng!";
			}
			catch
			{
				TempData["ErrorMessage"] = "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.";
			}
			
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return Redirect(returnUrl);
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Update(string productId, int quantity)
		{
			try
			{
				var cartId = EnsureCartId();
				await _api.PutAsync("api/public/Cart/items", new { ProductId = productId, Quantity = Math.Max(0, quantity), CartId = cartId });
				TempData["SuccessMessage"] = "Đã cập nhật giỏ hàng!";
			}
			catch
			{
				TempData["ErrorMessage"] = "Không thể cập nhật giỏ hàng. Vui lòng thử lại.";
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Remove(string productId)
		{
			try
			{
				var cartId = EnsureCartId();
				await _api.DeleteAsync($"api/public/Cart/items/{productId}?cartId={cartId}");
				TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
			}
			catch
			{
				TempData["ErrorMessage"] = "Không thể xóa sản phẩm. Vui lòng thử lại.";
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public async Task<IActionResult> Clear()
		{
			try
			{
				var cartId = EnsureCartId();
				await _api.PostAsync<object>($"api/public/Cart/clear?cartId={cartId}", new { });
				TempData["SuccessMessage"] = "Đã xóa tất cả sản phẩm khỏi giỏ hàng!";
			}
			catch
			{
				TempData["ErrorMessage"] = "Không thể xóa giỏ hàng. Vui lòng thử lại.";
			}
			return RedirectToAction("Index");
		}
	}
}

