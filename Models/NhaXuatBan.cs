using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class NhaXuatBan
{
    public int NhaXuatBanId { get; set; }

    public string TenNxb { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? DienThoai { get; set; }

    public virtual ICollection<DauSach> DauSaches { get; set; } = new List<DauSach>();
}
