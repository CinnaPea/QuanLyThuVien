using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class TaiKhoan
{
    public int TaiKhoanId { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public int VaiTroId { get; set; }

    public int? DocGiaId { get; set; }

    public bool TrangThai { get; set; }

    public virtual DocGium? DocGia { get; set; }

    public virtual VaiTro VaiTro { get; set; } = null!;
}
