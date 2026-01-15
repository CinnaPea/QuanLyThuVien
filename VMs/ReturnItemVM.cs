namespace WebApplication1.VMs
{
    public class ReturnItemVM
    {
        public int SachId { get; set; }
        public string? DauSachTitle { get; set; }
        public string? CurrentTinhTrang { get; set; } 
        public string TinhTrangLucTra { get; set; } = "Con"; 
        public bool IsReturned { get; set; } = true; 
    }

    public class ReturnFormVM
    {
        public int PhieuMuonId { get; set; }
        public int DocGiaId { get; set; }
        public string? DocGiaName { get; set; }
        public DateOnly HanTra { get; set; } 
        public string? GhiChu { get; set; }

        public List<ReturnItemVM> Items { get; set; } = new();
    }

}
