using FPTDrink.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FPTDrink.Web.ViewComponents
{
	public class CategoriesMenuViewComponent : ViewComponent
	{
		private readonly ApiClient _api;
		public CategoriesMenuViewComponent(ApiClient api)
		{
			_api = api;
		}

		public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken)
		{
			var cats = await _api.GetAsync<List<CategoryItem>>("api/public/Catalog/categories", cancellationToken) ?? new List<CategoryItem>();
			return View(cats);
		}

		public class CategoryItem
		{
			public int Id { get; set; }
			public string? Title { get; set; }
			public string? Alias { get; set; }
		}
	}
}

