using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Sach
{
    public int SachId { get; set; }

    public int DauSachId { get; set; }

    public string MaVach { get; set; } = null!;

    public string? ViTriKe { get; set; }

    public DateOnly? NgayNhap { get; set; }

    public string TinhTrang { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual ICollection<CtPhieuMuon> CtPhieuMuons { get; set; } = new List<CtPhieuMuon>();

    public virtual ICollection<CtPhieuTra> CtPhieuTras { get; set; } = new List<CtPhieuTra>();

    public virtual DauSach DauSach { get; set; } = null!;

    public virtual ICollection<Phat> Phats { get; set; } = new List<Phat>();
}
