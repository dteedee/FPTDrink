using System;
using System.Collections.Generic;

namespace FPTDrink.Core.Models;

public partial class KhachHang
{
    public string Id { get; set; } = null!;

    public string TenDangNhap { get; set; } = null!;

    public string? MatKhau { get; set; }

    public string Email { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public DateTime? NgaySinh { get; set; }

    public bool? GioiTinh { get; set; }

    public string? DiaChi { get; set; }

    public string? Cccd { get; set; }

    public string? Image { get; set; }

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    public string? EmailVerificationOtp { get; set; }

    public DateTime? EmailVerificationOtpExpiry { get; set; }

    public int EmailVerificationAttempts { get; set; }

    public string? GoogleId { get; set; }

    public string? GoogleEmail { get; set; }

    public bool IsGoogleAccount { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetExpiry { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public int Status { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public virtual ICollection<GioHang> GioHangs { get; set; } = new List<GioHang>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
}

