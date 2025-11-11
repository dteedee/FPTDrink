using FPTDrink.Web.Services;
using FPTDrink.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FPTDrink.Web.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApiClient _api;

        public MenuController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, int? categoryId = null, int? supplierId = null, decimal? priceFrom = null, decimal? priceTo = null, string? sort = null, string? q = null)
        {
            var categories = await _api.GetAsync<List<MenuCategoryItemViewModel>>("api/public/Catalog/categories") ?? new List<MenuCategoryItemViewModel>();
            var suppliersResponse = await _api.GetAsync<List<SupplierResponse>>("api/public/Suppliers") ?? new List<SupplierResponse>();
            var suppliers = suppliersResponse.Select(x => new MenuSupplierItemViewModel { Id = x.MaNhaCungCap, Title = x.Title }).ToList();

            var cartId = Request.Cookies["cart_id"];
            
            var queryParts = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (categoryId.HasValue) queryParts.Add($"categoryId={categoryId}");
            if (supplierId.HasValue) queryParts.Add($"supplierId={supplierId}");
            if (priceFrom.HasValue) queryParts.Add($"priceFrom={priceFrom}");
            if (priceTo.HasValue) queryParts.Add($"priceTo={priceTo}");
            if (!string.IsNullOrWhiteSpace(sort)) queryParts.Add($"sort={WebUtility.UrlEncode(sort)}");
            if (!string.IsNullOrWhiteSpace(q)) queryParts.Add($"q={WebUtility.UrlEncode(q)}");
            if (!string.IsNullOrWhiteSpace(cartId)) queryParts.Add($"cartId={WebUtility.UrlEncode(cartId)}");

            string productsUrl = "api/public/Catalog/products";
            if (queryParts.Count > 0)
            {
                productsUrl += "?" + string.Join("&", queryParts);
            }

            var products = await _api.GetAsync<ProductsListViewModel>(productsUrl) ?? new ProductsListViewModel();
            products.Page = page;
            products.PageSize = pageSize;
            products.CategoryId = categoryId;
            products.SupplierId = supplierId;
            products.PriceFrom = priceFrom;
            products.PriceTo = priceTo;
            products.Sort = sort;
            products.Q = q;

            var priceOptions = BuildPriceOptions(priceFrom, priceTo);
            var sortOptions = BuildSortOptions(sort);

            var vm = new MenuPageViewModel
            {
                Categories = categories,
                Suppliers = suppliers,
                Products = products,
                SelectedCategoryId = categoryId,
                SelectedSupplierId = supplierId,
                SelectedPriceFrom = priceFrom,
                SelectedPriceTo = priceTo,
                Sort = sort,
                Search = q,
                PriceRanges = priceOptions,
                SortOptions = sortOptions
            };

            ViewData["Title"] = "Thực đơn";
            return View(vm);
        }

        private static List<PriceRangeOptionViewModel> BuildPriceOptions(decimal? selectedFrom, decimal? selectedTo)
        {
            var options = new List<PriceRangeOptionViewModel>
            {
                new PriceRangeOptionViewModel { Key = "all", Label = "Tất cả", From = null, To = null },
                new PriceRangeOptionViewModel { Key = "0-30000", Label = "Dưới 30.000đ", From = null, To = 30000 },
                new PriceRangeOptionViewModel { Key = "30000-60000", Label = "30.000đ - 60.000đ", From = 30000, To = 60000 },
                new PriceRangeOptionViewModel { Key = "60000-100000", Label = "60.000đ - 100.000đ", From = 60000, To = 100000 },
                new PriceRangeOptionViewModel { Key = "100000-200000", Label = "100.000đ - 200.000đ", From = 100000, To = 200000 },
                new PriceRangeOptionViewModel { Key = "200000+", Label = "Trên 200.000đ", From = 200000, To = null }
            };

            foreach (var option in options)
            {
                option.Selected = Nullable.Equals(option.From, selectedFrom) && Nullable.Equals(option.To, selectedTo);
            }

            return options;
        }

        private static List<SortOptionViewModel> BuildSortOptions(string? current)
        {
            var options = new List<SortOptionViewModel>
            {
                new SortOptionViewModel { Value = string.Empty, Label = "Mặc định" },
                new SortOptionViewModel { Value = "price_asc", Label = "Giá tăng dần" },
                new SortOptionViewModel { Value = "price_desc", Label = "Giá giảm dần" },
                new SortOptionViewModel { Value = "name_asc", Label = "Tên A-Z" },
                new SortOptionViewModel { Value = "name_desc", Label = "Tên Z-A" }
            };

            foreach (var option in options)
            {
                option.Selected = string.Equals(option.Value, current, StringComparison.OrdinalIgnoreCase) ||
                                   (string.IsNullOrWhiteSpace(option.Value) && string.IsNullOrWhiteSpace(current));
            }

            return options;
        }

        private class SupplierResponse
        {
            public int MaNhaCungCap { get; set; }
            public string? Title { get; set; }
        }
    }
}
