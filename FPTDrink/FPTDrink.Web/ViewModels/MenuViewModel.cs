using System.Collections.Generic;

namespace FPTDrink.Web.ViewModels
{
    public class MenuPageViewModel
    {
        public List<MenuCategoryItemViewModel> Categories { get; set; } = new();
        public List<MenuSupplierItemViewModel> Suppliers { get; set; } = new();
        public List<PriceRangeOptionViewModel> PriceRanges { get; set; } = new();
        public List<SortOptionViewModel> SortOptions { get; set; } = new();
        public ProductsListViewModel Products { get; set; } = new();

        public int? SelectedCategoryId { get; set; }
        public string? SelectedSupplierId { get; set; }
        public decimal? SelectedPriceFrom { get; set; }
        public decimal? SelectedPriceTo { get; set; }
        public string? Sort { get; set; }
        public string? Search { get; set; }
    }

    public class MenuCategoryItemViewModel
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Alias { get; set; }
        public string? Image { get; set; }
    }

    public class MenuSupplierItemViewModel
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
    }

    public class PriceRangeOptionViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal? From { get; set; }
        public decimal? To { get; set; }
        public bool Selected { get; set; }
    }

    public class SortOptionViewModel
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}
