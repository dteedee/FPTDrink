using System;
using System.Collections.Generic;

namespace FPTDrink.Infrastructure.Models;

public partial class ThongKe
{
    public int Id { get; set; }

    public DateTime ThoiGian { get; set; }

    public long SoTruyCap { get; set; }

    public string? IdNhanVien { get; set; }

    public virtual NhanVien? IdNhanVienNavigation { get; set; }
}
