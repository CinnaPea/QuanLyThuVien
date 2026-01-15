using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.VMs
{
    public class DauSachFormVM
    {
        public int DauSachId { get; set; }

        [Required]
        public string? TieuDe { get; set; }

        public string? Isbn { get; set; }
        public int? NamXuatBan { get; set; }
        public string? NgonNgu { get; set; }
        public string? MoTa { get; set; }

        public int? TheLoaiId { get; set; }
        public int? NhaXuatBanId { get; set; }

        // Many-to-many authors: dropdown (multi)
        public List<int> TacGiaIds { get; set; } = new();
    }
}
