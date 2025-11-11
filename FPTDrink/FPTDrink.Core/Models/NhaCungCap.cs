using System;
using System.Collections.Generic;

namespace FPTDrink.Core.Models;

public partial class NhaCungCap
{
    public string MaNhaCungCap { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Alias { get; set; } = null!;

    public string? Image { get; set; }

    public string SoDienThoai { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }

    public string? IdNhanVien { get; set; }

    public virtual NhanVien? IdNhanVienNavigation { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
