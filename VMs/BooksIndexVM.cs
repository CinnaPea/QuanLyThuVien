using WebApplication1.Models;

namespace WebApplication1.VMs
{
    public class BooksIndexVM
    {
        public string? Q { get; set; }
        public int? TheLoaiId { get; set; }
        public int? NhaXuatBanId { get; set; }
        public int? TacGiaId { get; set; }

        public List<TheLoai> TheLoais { get; set; } = new();
        public List<NhaXuatBan> NhaXuatBans { get; set; } = new();
        public List<TacGium> TacGias { get; set; } = new();

        public List<DauSach> Items { get; set; } = new();

        // map: DauSachId -> "tg1, tg2..."
        public Dictionary<int, string> TacGiaTextByDauSachId { get; set; } = new();
    }
}
