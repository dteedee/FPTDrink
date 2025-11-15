using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FPTDrink.API.DTOs.Public.Cart;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class CartController : ControllerBase
	{
		private readonly ICartService _cartService;
		private readonly ICartMergeService _cartMergeService;

		public CartController(ICartService cartService, ICartMergeService cartMergeService)
		{
			_cartService = cartService;
			_cartMergeService = cartMergeService;
		}

		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] string? cartId, CancellationToken ct)
		{
			if (IsCustomer(out var customerId))
			{
				var cart = await BuildCustomerCartAsync(customerId!, ct);
				return Ok(cart);
			}

			return Ok(_cartService.GetCart(cartId));
		}

		[HttpPost("items")]
		public async Task<IActionResult> Add([FromBody] AddToCartRequest req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			if (IsCustomer(out var customerId))
			{
				var (success, error) = await _cartMergeService.AddItemAsync(customerId!, req.ProductId, req.Quantity, ct);
				if (!success) return BadRequest(new { message = error });
				var cart = await BuildCustomerCartAsync(customerId!, ct);
				return Ok(cart);
			}

			var guestCart = _cartService.Add(req.CartId, req.ProductId, req.Quantity);
			return Ok(guestCart);
		}

		[HttpPut("items")]
		public async Task<IActionResult> Update([FromBody] UpdateCartItemRequest req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			if (IsCustomer(out var customerId))
			{
				var (success, error) = await _cartMergeService.UpdateItemQuantityAsync(customerId!, req.ProductId, req.Quantity, ct);
				if (!success) return BadRequest(new { message = error });
				var cart = await BuildCustomerCartAsync(customerId!, ct);
				return Ok(cart);
			}

			var guestCart = _cartService.Update(req.CartId, req.ProductId, req.Quantity);
			return Ok(guestCart);
		}

		[HttpDelete("items/{productId}")]
		public async Task<IActionResult> Delete([FromQuery] string cartId, [FromRoute] string productId, CancellationToken ct)
		{
			if (IsCustomer(out var customerId))
			{
				var (success, error) = await _cartMergeService.RemoveItemAsync(customerId!, productId, ct);
				if (!success) return BadRequest(new { message = error });
				var cart = await BuildCustomerCartAsync(customerId!, ct);
				return Ok(cart);
			}

			return Ok(_cartService.Delete(cartId, productId));
		}

		[HttpPost("clear")]
		public async Task<IActionResult> Clear([FromQuery] string cartId, CancellationToken ct)
		{
			if (IsCustomer(out var customerId))
			{
				await _cartMergeService.ClearAsync(customerId!, ct);
				var cart = await BuildCustomerCartAsync(customerId!, ct);
				return Ok(cart);
			}

			return Ok(_cartService.Clear(cartId));
		}

		[HttpPost("merge")]
		[Authorize(Policy = "Customer")]
		public async Task<IActionResult> MergeGuestCart([FromBody] MergeCartRequestDto req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrWhiteSpace(customerId)) return Forbid();
			var (success, error) = await _cartMergeService.MergeGuestCartAsync(customerId, req.GuestCartId, ct);
			if (!success) return BadRequest(new { message = error });
			var cart = await BuildCustomerCartAsync(customerId, ct);
			return Ok(cart);
		}

		private bool IsCustomer(out string? customerId)
		{
			customerId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
			return User?.Identity?.IsAuthenticated == true && User.IsInRole("Customer") && !string.IsNullOrWhiteSpace(customerId);
		}

		private async Task<CartDto> BuildCustomerCartAsync(string customerId, CancellationToken ct)
		{
			var entities = await _cartMergeService.GetCustomerCartAsync(customerId, ct);
			var cart = new CartDto
			{
				CartId = $"customer:{customerId}",
				Items = new List<CartItemDto>()
			};

			foreach (var entity in entities)
			{
				var product = entity.MaSanPhamNavigation;
				if (product == null) continue;
				var price = product.GiaBan ?? product.GiaNiemYet;
				cart.Items.Add(new CartItemDto
				{
					ProductId = product.MaSanPham,
					ProductName = product.Title,
					ProductImg = product.Image,
					Quantity = entity.SoLuong,
					Price = price,
					TotalPrice = price * entity.SoLuong,
					TonKho = Math.Max(0, product.SoLuong - entity.SoLuong)
				});
			}

			return cart;
		}
	}
}


