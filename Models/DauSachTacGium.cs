using System;
using System.Collections.Generic;

namespace WebApplication1.Models;

public partial class DauSachTacGium
{
    public int DauSachId { get; set; }

    public int TacGiaId { get; set; }

    public string? VaiTro { get; set; }

    public virtual DauSach DauSach { get; set; } = null!;

    public virtual TacGium TacGia { get; set; } = null!;
}
