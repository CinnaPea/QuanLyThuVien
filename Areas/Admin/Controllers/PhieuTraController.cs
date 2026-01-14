using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PhieuTraController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public PhieuTraController(QuanLyThuVienContext db) => _db = db;

        // danh sách phiếu mượn đang mượn để trả
        public async Task<IActionResult> Index()
        {
            var list = await _db.PhieuMuons
                .Include(x => x.DocGia)
                .Where(x => x.TrangThai == "DangMuon")
                .OrderBy(x => x.PhieuMuonId)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> ReturnForm(int id) // id = PhieuMuonId
        {
            var phieu = await _db.PhieuMuons
                .Include(x => x.DocGia)
                .FirstOrDefaultAsync(x => x.PhieuMuonId == id);
            if (phieu == null) return NotFound();

            var ct = await _db.CtPhieuMuons
                .Include(x => x.Sach).ThenInclude(s => s.DauSach)
                .Where(x => x.PhieuMuonId == id)
                .ToListAsync();

            ViewBag.CT = ct;
            return View(phieu);
        }

        [HttpPost]
        public async Task<IActionResult> ReturnAll(int phieuMuonId)
        {
            var phieu = await _db.PhieuMuons.FindAsync(phieuMuonId);
            if (phieu == null) return NotFound();

            var ctMuon = await _db.CtPhieuMuons.Where(x => x.PhieuMuonId == phieuMuonId).ToListAsync();
            var sachIds = ctMuon.Select(x => x.SachId).ToList();
            var saches = await _db.Saches.Where(x => sachIds.Contains(x.SachId)).ToListAsync();

            // tạo Phiếu trả
            var phieuTra = new PhieuTra
            {
                PhieuMuonId = phieuMuonId,
                NgayTra = DateTime.Now
            };
            _db.PhieuTras.Add(phieuTra);
            await _db.SaveChangesAsync();

            // tạo CT_PhieuTra + cập nhật CT_PhieuMuon
            foreach (var ct in ctMuon)
            {
                ct.NgayTraThucTe = DateOnly.FromDateTime(DateTime.Now);

                ct.TinhTrangLucTra = "Con";

                _db.CtPhieuTras.Add(new CtPhieuTra
                {
                    PhieuTraId = phieuTra.PhieuTraId,
                    SachId = ct.SachId,
                    TinhTrangLucTra = "Con"
                });
            }

            // trả sách => còn
            foreach (var s in saches)
                s.TinhTrang = "Con";

            // update trạng thái phiếu mượn
            phieu.TrangThai = "DaTra";

            await _db.SaveChangesAsync();

            // phạt trễ hạn (nếu có HanTra)
            var today = DateOnly.FromDateTime(DateTime.Now);

            if (today > phieu.HanTra)
            {
                int soNgayTre = today.DayNumber - phieu.HanTra.DayNumber;

                decimal tienPhat = soNgayTre * 5000m;

                foreach (var sid in sachIds)
                {
                    _db.Phats.Add(new Phat
                    {
                        PhieuMuonId = phieuMuonId,
                        SachId = sid,
                        LyDo = $"Trễ hạn {soNgayTre} ngày",
                        SoTien = tienPhat,
                        DaThanhToan = false,
                        NgayLap = DateTime.Now,
                        GhiChu = null
                    });
                }
            await _db.SaveChangesAsync();
            }

            TempData["ok"] = "Đã lập phiếu trả.";
            return RedirectToAction(nameof(Index));
        }
    }
}
