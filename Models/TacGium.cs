using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class TacGium
{
    public int TacGiaId { get; set; }

    public string TenTacGia { get; set; } = null!;

    public virtual ICollection<DauSachTacGium> DauSachTacGia { get; set; } = new List<DauSachTacGium>();
}
