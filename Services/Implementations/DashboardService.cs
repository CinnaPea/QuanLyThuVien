using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly QuanLyThuVienContext _context;

        public DashboardService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public int TongSoSach()
            => _context.Saches.Count();

        public int SachDangMuon()
            => _context.CtPhieuMuons
                .Count(ctm =>
                    !_context.CtPhieuTras.Any(ctt => ctt.SachId == ctm.SachId));

        public int PhieuMuonQuaHan()
        {
            var today = DateOnly.FromDayNumber(DateTime.Today.Day);

            return _context.PhieuMuons
                .Count(pm => pm.TrangThai == "Active" && pm.HanTra < today);
        }

        public int DocGiaDangHoatDong()
            => _context.DocGia.Count(dg => dg.TrangThai);

        public decimal TongTienPhatChuaThanhToan()
            => _context.Phats
                .Sum(p =>
                    p.SoTien -
                    _context.ThanhToanPhats
                        .Where(t => t.PhatId == p.PhatId)
                        .Sum(t => t.SoTien));

    }
}
