using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class FineService : IFineService
    {
        private readonly QuanLyThuVienContext _context;

        public FineService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public Phat LapPhatNeuQuaHan(int phieuMuonId)
        {
            var pm = _context.PhieuMuons.Find(phieuMuonId);
            var today = DateTime.Now;

            if (pm.HanTra >= DateOnly.FromDayNumber(DateTime.Today.Day))
                return null;

            var soNgayTre = today.Day - pm.HanTra.DayNumber;
            var soTien = soNgayTre * 5000;

            var phat = new Phat
            {
                PhieuMuonId = phieuMuonId,
                SoTien = soTien,
                NgayLap = today
            };

            _context.Phats.Add(phat);
            _context.SaveChanges();

            return phat;
        }

        public void ThanhToanPhat(int phatId, decimal soTien)
        {
            _context.ThanhToanPhats.Add(new ThanhToanPhat
            {
                PhatId = phatId,
                SoTien = soTien,
                NgayThanhToan = DateTime.Now
            });

            _context.SaveChanges();
        }
    }

}
