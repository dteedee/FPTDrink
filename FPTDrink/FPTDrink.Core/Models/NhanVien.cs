using System;
using System.Collections.Generic;

namespace FPTDrink.Infrastructure.Models;

public partial class NhanVien
{
    public string Id { get; set; } = null!;

    public int? IdChucVu { get; set; }

    public string FullName { get; set; } = null!;

    public DateTime NgaySinh { get; set; }

    public string? Image { get; set; }

    public bool GioiTinh { get; set; }

    public string DiaChi { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Cccd { get; set; } = null!;

    public int Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string TenHienThi { get; set; } = null!;

    public bool IsActiveAccount { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ChucVu? IdChucVuNavigation { get; set; }

    public virtual ICollection<NhaCungCap> NhaCungCaps { get; set; } = new List<NhaCungCap>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<ThongKe> ThongKes { get; set; } = new List<ThongKe>();
}
