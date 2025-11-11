using System;
using System.Collections.Generic;

namespace FPTDrink.Infrastructure.Models;

public partial class Product
{
    public string MaSanPham { get; set; } = null!;

    public string ProductCategoryId { get; set; } = null!;

    public string SupplierId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Alias { get; set; } = null!;

    public string? Image { get; set; }

    public string? MoTa { get; set; }

    public string? ChiTiet { get; set; }

    public decimal GiaNhap { get; set; }

    public decimal GiaNiemYet { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? GiamGia { get; set; }

    public int SoLuong { get; set; }

    public int ViewCount { get; set; }

    public bool IsHome { get; set; }

    public bool IsSale { get; set; }

    public bool IsNew { get; set; }

    public bool IsHot { get; set; }

    public bool IsActive { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }

    public int Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public string? IdNhanVien { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual NhanVien? IdNhanVienNavigation { get; set; }

    public virtual ProductCategory ProductCategory { get; set; } = null!;

    public virtual NhaCungCap Supplier { get; set; } = null!;
}
