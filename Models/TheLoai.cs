using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class TheLoai
{
    public int TheLoaiId { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public string? MoTa { get; set; }

    public virtual ICollection<DauSach> DauSaches { get; set; } = new List<DauSach>();
}
