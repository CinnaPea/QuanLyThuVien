using WebApplication1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SachController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public SachController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var list = await _db.Saches
                .Include(x => x.DauSach)
                .OrderByDescending(x => x.SachId)
                .ToListAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.DauSachId = new SelectList(_db.DauSaches, "DauSachId", "TieuDe");
            return View(new Sach { TinhTrang = "Con" });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Sach model)
        {
            _db.Saches.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Saches.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.DauSachId = new SelectList(_db.DauSaches, "DauSachId", "TieuDe", item.DauSachId);
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Sach model)
        {
            _db.Saches.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Saches.FindAsync(id);
            if (item == null) return NotFound();
            _db.Saches.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
