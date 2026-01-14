using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DauSachController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public DauSachController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var list = await _db.DauSaches
                .Include(x => x.TheLoai)
                .Include(x => x.NhaXuatBan)
                .OrderByDescending(x => x.DauSachId)
                .ToListAsync();
            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.TheLoaiId = new SelectList(_db.TheLoais, "TheLoaiId", "TenTheLoai");
            ViewBag.NhaXuatBanId = new SelectList(_db.NhaXuatBans, "NhaXuatBanId", "TenNXB");
            return View(new DauSach());
        }

        [HttpPost]
        public async Task<IActionResult> Create(DauSach model)
        {
            _db.DauSaches.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.DauSaches.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.TheLoaiId = new SelectList(_db.TheLoais, "TheLoaiId", "TenTheLoai", item.TheLoaiId);
            ViewBag.NhaXuatBanId = new SelectList(_db.NhaXuatBans, "NhaXuatBanId", "TenNXB", item.NhaXuatBanId);
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DauSach model)
        {
            _db.DauSaches.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.DauSaches.FindAsync(id);
            if (item == null) return NotFound();
            _db.DauSaches.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
