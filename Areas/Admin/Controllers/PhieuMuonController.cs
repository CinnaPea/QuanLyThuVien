using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PhieuMuonController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public PhieuMuonController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> ChoDuyet()
        {
            var list = await _db.PhieuMuons
                .Include(x => x.DocGia)
                .Where(x => x.TrangThai == "ChoDuyet")
                .OrderByDescending(x => x.PhieuMuonId)
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(int id)
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
        public async Task<IActionResult> Approve(int id, int soNgayMuon = 7)
        {
            var phieu = await _db.PhieuMuons.FindAsync(id);
            if (phieu == null) return NotFound();

            var ct = await _db.CtPhieuMuons.Where(x => x.PhieuMuonId == id).ToListAsync();
            var sachIds = ct.Select(x => x.SachId).ToList();
            var saches = await _db.Saches.Where(x => sachIds.Contains(x.SachId)).ToListAsync();

            if (saches.Any(s => !(s.TinhTrang == "Con" || s.TinhTrang == "Available")))
            {
                TempData["err"] = "Có sách không còn khả dụng nên không thể duyệt.";
                return RedirectToAction(nameof(Details), new { id });
            }

            phieu.TrangThai = "DangMuon";
            phieu.HanTra = DateOnly.FromDateTime(DateTime.Now.AddDays(soNgayMuon));

            foreach (var s in saches) s.TinhTrang = "DangMuon";

            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã duyệt & giao sách.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string? ghiChu)
        {
            var phieu = await _db.PhieuMuons.FindAsync(id);
            if (phieu == null) return NotFound();

            phieu.TrangThai = "TuChoi";
            phieu.GhiChu = ghiChu;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ChoDuyet));
        }
    }
}
