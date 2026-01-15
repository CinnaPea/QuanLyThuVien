namespace WebApplication1.VMs
{
    public class DauSachListItemVM
    {
        public int DauSachId { get; set; }
        public string? TieuDe { get; set; }
        public string? ISBN { get; set; }
        public string? TenTheLoai { get; set; }
        public string? TenNXB { get; set; }
        public string? TacGiaText { get; set; }

        public int TotalCount { get; set; }
        public int AvailableCount { get; set; }
    }
}
