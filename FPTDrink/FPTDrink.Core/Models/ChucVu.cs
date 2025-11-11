using System;
using System.Collections.Generic;

namespace FPTDrink.Infrastructure.Models;

public partial class ChucVu
{
    public int Id { get; set; }

    public string TenChucVu { get; set; } = null!;

    public string? MoTa { get; set; }

    public int Status { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? Modifiedby { get; set; }

    public virtual ICollection<NhanVien> NhanViens { get; set; } = new List<NhanVien>();

    public virtual ICollection<PhanQuyen> PhanQuyens { get; set; } = new List<PhanQuyen>();
}
