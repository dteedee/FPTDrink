using System;
using System.Text.Json.Serialization;

namespace FPTDrink.Web.ViewModels
{
    public class ProductListItemViewModel
    {
        [JsonPropertyName("maSanPham")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        [JsonPropertyName("giaHienThi")]
        public decimal GiaHienThi { get; set; }

        [JsonPropertyName("giamGia")]
        public decimal? GiamGia { get; set; }

        [JsonPropertyName("isSale")]
        public bool IsSale { get; set; }

        [JsonPropertyName("productCategoryTitle")]
        public string? CategoryTitle { get; set; }

        [JsonPropertyName("supplierTitle")]
        public string? SupplierTitle { get; set; }

        [JsonPropertyName("soLuong")]
        public int SoLuong { get; set; }

        [JsonIgnore]
        public bool HasDiscount => GiamGia.HasValue && GiamGia.Value > 0;

        [JsonIgnore]
        public decimal OriginalPrice => HasDiscount ? GiaHienThi + GiamGia!.Value : GiaHienThi;

        [JsonIgnore]
        public int DiscountPercent => HasDiscount && OriginalPrice > 0
            ? (int)Math.Round((double)(GiamGia!.Value / OriginalPrice * 100))
            : 0;
    }
}

