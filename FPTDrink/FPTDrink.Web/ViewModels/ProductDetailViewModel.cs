using System;
using System.Text.Json.Serialization;

namespace FPTDrink.Web.ViewModels
{
    public class ProductDetailViewModel
    {
        [JsonPropertyName("maSanPham")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("moTa")]
        public string? Description { get; set; }

        [JsonPropertyName("chiTiet")]
        public string? Content { get; set; }

        [JsonPropertyName("giaNiemYet")]
        public decimal ListPrice { get; set; }

        [JsonPropertyName("giaBan")]
        public decimal? SalePrice { get; set; }

        [JsonPropertyName("giamGia")]
        public decimal? DiscountAmount { get; set; }

        [JsonPropertyName("giaNhap")]
        public decimal CostPrice { get; set; }

        [JsonPropertyName("soLuong")]
        public int Quantity { get; set; }

        [JsonPropertyName("productCategoryTitle")]
        public string? CategoryTitle { get; set; }

        [JsonPropertyName("supplierTitle")]
        public string? SupplierTitle { get; set; }

        [JsonIgnore]
        public decimal DisplayPrice => SalePrice ?? ListPrice;

        [JsonIgnore]
        public bool HasDiscount => SalePrice.HasValue && SalePrice.Value < ListPrice;

        [JsonIgnore]
        public int DiscountPercent => HasDiscount && ListPrice > 0
            ? (int)Math.Round((double)((ListPrice - DisplayPrice) / ListPrice * 100))
            : 0;
    }
}
