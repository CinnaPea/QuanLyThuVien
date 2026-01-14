using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class VaiTro
{
    public int VaiTroId { get; set; }

    public string TenVaiTro { get; set; } = null!;

    public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
}
