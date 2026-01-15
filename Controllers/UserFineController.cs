using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class UserFineController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public UserFineController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var docGiaId = HttpContext.Session.GetInt32("DOCGIA_ID");
            if (docGiaId == null) return RedirectToAction("Login", "Account");

            // Phat -> PhieuMuon -> DocGiaId
            var list = await _db.Phats
                .Include(p => p.PhieuMuon)
                .AsNoTracking()
                .Where(p => p.PhieuMuon.DocGiaId == docGiaId.Value)
                .OrderByDescending(p => p.NgayLap)
                .ToListAsync();

            return View(list);
        }
    }
}
