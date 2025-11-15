using System;

namespace FPTDrink.Core.Models;

public partial class GioHangTam
{
    public int Id { get; set; }

    public string CartId { get; set; } = null!;

    public string MaSanPham { get; set; } = null!;

    public int SoLuong { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public virtual Product MaSanPhamNavigation { get; set; } = null!;
}

