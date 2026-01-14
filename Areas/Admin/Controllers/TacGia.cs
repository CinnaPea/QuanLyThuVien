using WebApplication1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TacGiaController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public TacGiaController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.TacGia.OrderBy(x => x.TenTacGia).ToListAsync());

        public IActionResult Create() => View(new TacGium());

        [HttpPost]
        public async Task<IActionResult> Create(TacGium model)
        {
            _db.TacGia.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TacGia.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TacGium model)
        {
            _db.TacGia.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TacGia.FindAsync(id);
            if (item == null) return NotFound();
            _db.TacGia.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
