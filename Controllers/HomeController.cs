using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1;

namespace LibraryManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public HomeController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index(string? q = null)
        {
            var query = _db.DauSaches
                .Include(x => x.TheLoai)
                .Include(x => x.NhaXuatBan)
                .Include(x => x.Saches)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.TieuDe.Contains(q) || (x.Isbn != null && x.Isbn.Contains(q)));

            return View(await query.OrderBy(x => x.TieuDe).ToListAsync());
        }
    }
}
