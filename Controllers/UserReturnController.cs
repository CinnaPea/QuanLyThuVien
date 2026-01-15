using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserReturnController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public UserReturnController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var docGiaId = HttpContext.Session.GetInt32("DOCGIA_ID");
            if (docGiaId == null) return RedirectToAction("Login", "Account");

            // PhieuTra -> PhieuMuon -> DocGiaId
            var list = await _db.PhieuTras
                .Include(pt => pt.PhieuMuon)
                .AsNoTracking()
                .Where(pt => pt.PhieuMuon.DocGiaId == docGiaId.Value)
                .OrderByDescending(pt => pt.NgayTra)
                .ToListAsync();

            return View(list);
        }

        // (Tuỳ chọn) Chi tiết phiếu trả + sách đã trả
        public async Task<IActionResult> Details(int id)
        {
            var docGiaId = HttpContext.Session.GetInt32("DOCGIA_ID");
            if (docGiaId == null) return RedirectToAction("Login", "Account");

            var phieuTra = await _db.PhieuTras
                .Include(pt => pt.PhieuMuon)
                .Include(pt => pt.CtPhieuTras)
                    .ThenInclude(ct => ct.Sach)
                        .ThenInclude(s => s.DauSach)
                .AsNoTracking()
                .FirstOrDefaultAsync(pt => pt.PhieuTraId == id && pt.PhieuMuon.DocGiaId == docGiaId.Value);

            if (phieuTra == null) return NotFound();
            return View(phieuTra);
        }
    }
}
