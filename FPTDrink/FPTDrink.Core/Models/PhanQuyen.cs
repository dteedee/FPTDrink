using System;
using System.Collections.Generic;

namespace FPTDrink.Core.Models;

public partial class PhanQuyen
{
    public string MaChucNang { get; set; } = null!;

    public int IdchucVu { get; set; }

    public string? GhiChu { get; set; }

    public virtual ChucVu IdchucVuNavigation { get; set; } = null!;

    public virtual ChucNangQuyen MaChucNangNavigation { get; set; } = null!;
}
