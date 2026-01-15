using WebApplication1.Models;

namespace WebApplication1.VMs
{
    public class PhieuTraIndexVM
    {
        public List<PhieuMuon> DangMuon { get; set; } = new();
        public List<PhieuTra> LichSuTra { get; set; } = new();
    }

}
