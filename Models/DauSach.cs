using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class DauSach
{
    public int DauSachId { get; set; }

    public string TieuDe { get; set; } = null!;

    public string? Isbn { get; set; }

    public int? NamXuatBan { get; set; }

    public string? NgonNgu { get; set; }

    public string? MoTa { get; set; }

    public int? TheLoaiId { get; set; }

    public int? NhaXuatBanId { get; set; }

    public virtual ICollection<DauSachTacGium> DauSachTacGia { get; set; } = new List<DauSachTacGium>();

    public virtual NhaXuatBan? NhaXuatBan { get; set; }

    public virtual ICollection<Sach> Saches { get; set; } = new List<Sach>();

    public virtual TheLoai? TheLoai { get; set; }
}
