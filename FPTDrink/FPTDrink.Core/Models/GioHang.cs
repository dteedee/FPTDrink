using System;

namespace FPTDrink.Core.Models;

public partial class GioHang
{
    public int Id { get; set; }

    public string IdKhachHang { get; set; } = null!;

    public string MaSanPham { get; set; } = null!;

    public int SoLuong { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual KhachHang IdKhachHangNavigation { get; set; } = null!;

    public virtual Product MaSanPhamNavigation { get; set; } = null!;
}

