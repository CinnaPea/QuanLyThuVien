using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class PhieuMuon
{
    public int PhieuMuonId { get; set; }

    public int DocGiaId { get; set; }

    public DateTime NgayMuon { get; set; }

    public DateOnly HanTra { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual ICollection<CtPhieuMuon> CtPhieuMuons { get; set; } = new List<CtPhieuMuon>();

    public virtual DocGium DocGia { get; set; } = null!;

    public virtual ICollection<Phat> Phats { get; set; } = new List<Phat>();

    public virtual ICollection<PhieuTra> PhieuTras { get; set; } = new List<PhieuTra>();

   
    
}
