using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class BorrowService : IBorrowService
    {
        private readonly QuanLyThuVienContext _context;

        public BorrowService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public bool SachDangDuocMuon(int sachId)
        {
            return _context.CtPhieuMuons
                .Any(ctm =>
                    ctm.SachId == sachId &&
                    !_context.CtPhieuTras.Any(ctt => ctt.SachId == sachId));
        }

        public int TaoPhieuMuon(int docGiaId, IEnumerable<int> sachIds, DateOnly hanTra)
        {
            using var tran = _context.Database.BeginTransaction();

            foreach (var sachId in sachIds)
            {
                if (SachDangDuocMuon(sachId))
                    throw new Exception($"Sách {sachId} đang được mượn");
            }

            var phieuMuon = new PhieuMuon
            {
                DocGiaId = docGiaId,
                NgayMuon = DateTime.Now,
                HanTra = hanTra,
                TrangThai = "Active"
            };

            _context.PhieuMuons.Add(phieuMuon);
            _context.SaveChanges();

            foreach (var sachId in sachIds)
            {
                _context.CtPhieuMuons.Add(new CtPhieuMuon 
                {
                    PhieuMuonId = phieuMuon.PhieuMuonId,
                    SachId = sachId
                });
            }

            _context.SaveChanges();
            tran.Commit();

            return phieuMuon.PhieuMuonId;
        }

        public void TraSach(int phieuMuonId, IEnumerable<int> sachIds)
        {
            using var tran = _context.Database.BeginTransaction();

            var phieuTra = new PhieuTra
            {
                PhieuMuonId = phieuMuonId,
                NgayTra = DateTime.Now
            };

            _context.PhieuTras.Add(phieuTra);
            _context.SaveChanges();

            foreach (var sachId in sachIds)
            {
                _context.CtPhieuTras.Add(new CtPhieuTra
                {
                    PhieuTraId = phieuTra.PhieuTraId,
                    SachId = sachId
                });
            }

            var pm = _context.PhieuMuons.Find(phieuMuonId);
            pm.TrangThai = "Completed";

            _context.SaveChanges();
            tran.Commit();
        }
    }

}
