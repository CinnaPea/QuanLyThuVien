using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class CtPhieuTra
{
    public int PhieuTraId { get; set; }

    public int SachId { get; set; }

    public string? TinhTrangLucTra { get; set; }

    public virtual PhieuTra PhieuTra { get; set; } = null!;

    public virtual Sach Sach { get; set; } = null!;
}
