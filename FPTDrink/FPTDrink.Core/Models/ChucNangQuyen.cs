using System;
using System.Collections.Generic;

namespace FPTDrink.Core.Models;
public partial class ChucNangQuyen
{
    public string MaChucNang { get; set; } = null!;

    public string TenChucNang { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<PhanQuyen> PhanQuyens { get; set; } = new List<PhanQuyen>();
}
