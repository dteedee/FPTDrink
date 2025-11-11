using System;
using System.Collections.Generic;

namespace FPTDrink.Infrastructure.Models;

public partial class ChiTietHoaDon
{
    public int Id { get; set; }

    public string? OrderId { get; set; }

    public string? ProductId { get; set; }

    public decimal GiaBan { get; set; }

    public int GiamGia { get; set; }

    public int SoLuong { get; set; }

    public virtual HoaDon? Order { get; set; }

    public virtual Product? Product { get; set; }
}
