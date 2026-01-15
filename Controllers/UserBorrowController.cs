using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserBorrowController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public UserBorrowController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var docGiaId = HttpContext.Session.GetInt32("DOCGIA_ID");
            if (docGiaId == null) return RedirectToAction("Login", "Account");

            var list = await _db.PhieuMuons
                .AsNoTracking()
                .Where(p => p.DocGiaId == docGiaId.Value)
                .OrderByDescending(p => p.NgayMuon)
                .ToListAsync();

            return View(list);
        }

        // (Tuỳ chọn) xem chi tiết 1 phiếu mượn + sách trong phiếu
        public async Task<IActionResult> Details(int id)
        {
            var docGiaId = HttpContext.Session.GetInt32("DOCGIA_ID");
            if (docGiaId == null) return RedirectToAction("Login", "Account");

            var phieu = await _db.PhieuMuons
                .Include(p => p.CtPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                        .ThenInclude(s => s.DauSach)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PhieuMuonId == id && p.DocGiaId == docGiaId.Value);

            if (phieu == null) return NotFound();
            return View(phieu);
        }
    }
}
