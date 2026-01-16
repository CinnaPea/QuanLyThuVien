namespace WebApplication1.VMs
{
    public class DashboardVM
    {
        public int TongSach { get; set; }
        public int SachDangMuon { get; set; }
        public int PhieuMuonQuaHan { get; set; }
        public int DocGiaDangHoatDong { get; set; }

        // Giá trị thực tế từ DB (có thể âm)
        public decimal TongTienPhatChuaThanhToan { get; set; }

        // 👉 Giá trị dùng để HIỂN THỊ (không bao giờ âm)
        public decimal TongTienPhatHienThi
        {
            get
            {
                return TongTienPhatChuaThanhToan < 0
                    ? 0
                    : TongTienPhatChuaThanhToan;
            }
        }
    }
}
