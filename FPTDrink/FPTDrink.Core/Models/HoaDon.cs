using System;
using System.Collections.Generic;

namespace FPTDrink.Infrastructure.Models;

public partial class HoaDon
{
    public string MaHoaDon { get; set; } = null!;

    public string TenKhachHang { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string DiaChi { get; set; } = null!;

    public string? Email { get; set; }

    public int PhuongThucThanhToan { get; set; }

    public int TrangThai { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public string IdKhachHang { get; set; } = null!;

    public string Cccd { get; set; } = null!;

    public string? IdNhanVien { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual NhanVien? IdNhanVienNavigation { get; set; }
}
