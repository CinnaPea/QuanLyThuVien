using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class PhieuTra
{
    public int PhieuTraId { get; set; }

    public int PhieuMuonId { get; set; }

    public DateTime NgayTra { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<CtPhieuTra> CtPhieuTras { get; set; } = new List<CtPhieuTra>();

    public virtual PhieuMuon PhieuMuon { get; set; } = null!;
}
