using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class CtPhieuMuon
{
    public int PhieuMuonId { get; set; }

    public int SachId { get; set; }

    public DateOnly? NgayTraThucTe { get; set; }

    public string? TinhTrangLucMuon { get; set; }

    public string? TinhTrangLucTra { get; set; }

    public virtual PhieuMuon PhieuMuon { get; set; } = null!;

    public virtual Sach Sach { get; set; } = null!;
}
