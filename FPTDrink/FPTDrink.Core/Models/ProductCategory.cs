using System;
using System.Collections.Generic;

namespace FPTDrink.Infrastructure.Models;

public partial class ProductCategory
{
    public string MaLoaiSanPham { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Alias { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? Image { get; set; }

    public bool IsActive { get; set; }

    public int Status { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
