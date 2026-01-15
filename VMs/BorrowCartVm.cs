namespace WebApplication1.VMs
{
    // item lưu trong session
    public class BorrowCartItemDto
    {
        public int DauSachId { get; set; }
        public int Qty { get; set; }
        public bool Selected { get; set; } = true; // mặc định tick
    }

    // item hiển thị ra view
    public class BorrowCartItemVM
    {
        public int DauSachId { get; set; }
        public string? TieuDe { get; set; }
        public string? TheLoai { get; set; }
        public string? NXB { get; set; }
        public int Available { get; set; }
        public int Qty { get; set; }
        public bool Selected { get; set; }
    }

    public class BorrowCartVM
    {
        public List<BorrowCartItemVM> Items { get; set; } = new();
        public int TotalSelectedQty => Items.Where(x => x.Selected).Sum(x => x.Qty);
    }
}

