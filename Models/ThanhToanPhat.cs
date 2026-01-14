using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class ThanhToanPhat
{
    public int ThanhToanPhatId { get; set; }

    public int PhatId { get; set; }

    public decimal SoTien { get; set; }

    public DateTime NgayThanhToan { get; set; }

    public string? GhiChu { get; set; }

    public virtual Phat Phat { get; set; } = null!;
}
