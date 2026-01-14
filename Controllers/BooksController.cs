using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace LibraryManagement.Controllers
{
    public class BooksController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public BooksController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Details(int id)
        {
            var dauSach = await _db.DauSaches
                .Include(x => x.TheLoai)
                .Include(x => x.NhaXuatBan)
                .FirstOrDefaultAsync(x => x.DauSachId == id);

            if (dauSach == null) return NotFound();

            var sachCon = await _db.Saches
                .Where(s => s.DauSachId == id && (s.TinhTrang == "Con" || s.TinhTrang == "Available"))
                .OrderBy(s => s.SachId)
                .ToListAsync();

            ViewBag.SachCon = sachCon;
            return View(dauSach);
        }
    }
}
