using System.ComponentModel.DataAnnotations;

namespace WebApplication1.VMs
{
    public class SachBatchVM
    {
        public int DauSachId { get; set; }
        public string? DauSachTieuDe { get; set; }

        [Range(1, 500, ErrorMessage = "Số lượng phải từ 1 đến 500.")]
        public int SoLuong { get; set; } = 1;

        [StringLength(100)]
        public string? ViTriKe { get; set; }

        public DateOnly? NgayNhap { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        public string TinhTrang { get; set; } = "Con";

        [StringLength(255)]
        public string? GhiChu { get; set; }
    }
}
