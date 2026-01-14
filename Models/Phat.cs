using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class Phat
{
    public int PhatId { get; set; }

    public int PhieuMuonId { get; set; }

    public int? SachId { get; set; }

    public string LyDo { get; set; } = null!;

    public decimal SoTien { get; set; }

    public bool DaThanhToan { get; set; }

    public DateTime NgayLap { get; set; }

    public string? GhiChu { get; set; }

    public virtual PhieuMuon PhieuMuon { get; set; } = null!;

    public virtual Sach? Sach { get; set; }

    public virtual ICollection<ThanhToanPhat> ThanhToanPhats { get; set; } = new List<ThanhToanPhat>();
}
