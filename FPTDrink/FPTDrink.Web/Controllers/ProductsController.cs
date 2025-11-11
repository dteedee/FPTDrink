using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FPTDrink.Web.Controllers
{
	public class ProductsController : Controller
	{
		private readonly ApiClient _api;
		public ProductsController(ApiClient api)
		{
			_api = api;
		}

		public async Task<IActionResult> Index(int page = 1, int pageSize = 12, int? categoryId = null, int? supplierId = null, string? q = null, decimal? priceFrom = null, decimal? priceTo = null, string? sort = null)
		{
			var cartId = Request.Cookies["cart_id"];
			var cartIdParam = !string.IsNullOrWhiteSpace(cartId) ? $"&cartId={System.Net.WebUtility.UrlEncode(cartId)}" : "";
			string url = $"api/public/Catalog/products?page={page}&pageSize={pageSize}&categoryId={categoryId}&supplierId={supplierId}&q={System.Net.WebUtility.UrlEncode(q)}&priceFrom={priceFrom}&priceTo={priceTo}&sort={sort}{cartIdParam}";
			var vm = await _api.GetAsync<ProductsListViewModel>(url) ?? new ProductsListViewModel();
			vm.Page = page;
			vm.PageSize = pageSize;
			vm.CategoryId = categoryId;
			vm.SupplierId = supplierId;
			vm.Q = q;
			vm.PriceFrom = priceFrom;
			vm.PriceTo = priceTo;
			vm.Sort = sort;
			return View(vm);
		}

		public async Task<IActionResult> ProductCategory(string alias, int id, int page = 1, int pageSize = 12)
		{
			var cartId = Request.Cookies["cart_id"];
			var cartIdParam = !string.IsNullOrWhiteSpace(cartId) ? $"&cartId={System.Net.WebUtility.UrlEncode(cartId)}" : "";
			string url = $"api/public/Catalog/products?page={page}&pageSize={pageSize}&categoryId={id}{cartIdParam}";
			var vm = await _api.GetAsync<ProductsListViewModel>(url) ?? new ProductsListViewModel();
			vm.Page = page;
			vm.PageSize = pageSize;
			vm.CategoryId = id;
			ViewBag.CategoryAlias = alias;
			ViewBag.CategoryId = id;
			return View("Index", vm);
		}

		public async Task<IActionResult> Detail(string id)
		{
			var cartId = Request.Cookies["cart_id"];
			var cartIdParam = !string.IsNullOrWhiteSpace(cartId) ? $"&cartId={System.Net.WebUtility.UrlEncode(cartId)}" : "";
			var dto = await _api.GetAsync<ProductDetailDto>($"api/public/Products/{id}?related=8{cartIdParam}");
			if (dto == null) return NotFound();
			return View(dto);
		}

		public class ProductDetailDto
		{
			[System.Text.Json.Serialization.JsonPropertyName("detail")]
			public ProductDetailViewModel? Detail { get; set; }
			[System.Text.Json.Serialization.JsonPropertyName("related")]
			public List<ProductListItemViewModel> Related { get; set; } = new();
		}
	}
}

