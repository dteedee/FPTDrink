using FPTDrink.API.DTOs.Public.Cart;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class CartController : ControllerBase
	{
		private readonly ICartService _cartService;

		public CartController(ICartService cartService)
		{
			_cartService = cartService;
		}

		[HttpGet]
		public IActionResult Get([FromQuery] string? cartId)
		{
			var cart = _cartService.GetCart(cartId);
			return Ok(cart);
		}

		[HttpPost("items")]
		public IActionResult Add([FromBody] AddToCartRequest req)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var cart = _cartService.Add(req.CartId, req.ProductId, req.Quantity);
			return Ok(cart);
		}

		[HttpPut("items")]
		public IActionResult Update([FromBody] UpdateCartItemRequest req)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var cart = _cartService.Update(req.CartId, req.ProductId, req.Quantity);
			return Ok(cart);
		}

		[HttpDelete("items/{productId}")]
		public IActionResult Delete([FromQuery] string cartId, [FromRoute] string productId)
		{
			var cart = _cartService.Delete(cartId, productId);
			return Ok(cart);
		}

		[HttpPost("clear")]
		public IActionResult Clear([FromQuery] string cartId)
		{
			var cart = _cartService.Clear(cartId);
			return Ok(cart);
		}
	}
}


