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
                    !_context.PhieuTras
                        .Any(pt => pt.PhieuMuonId == ctm.PhieuMuonId));

        public int PhieuMuonQuaHan()
        {
            var today = DateOnly.FromDayNumber(DateTime.Today.Day);

            return _context.PhieuMuons
                .Count(pm => pm.HanTra < today);
        }

        public int DocGiaDangHoatDong()
            => _context.DocGia.Count(dg => dg.TrangThai);
        public decimal TongTienPhatChuaThanhToan()
        {
            var tongPhat = _context.Phats
                .Sum(p => (decimal?)p.SoTien) ?? 0;

            var tongDaTra = _context.ThanhToanPhats
                .Sum(t => (decimal?)t.SoTien) ?? 0;

            return tongPhat - tongDaTra;
        }

    }

}
