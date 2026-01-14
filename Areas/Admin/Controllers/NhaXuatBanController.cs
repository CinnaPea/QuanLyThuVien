using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NhaXuatBanController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public NhaXuatBanController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.NhaXuatBans.OrderBy(x => x.NhaXuatBanId).ToListAsync());
        public IActionResult Create() => View(new NhaXuatBan());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhaXuatBan model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _db.NhaXuatBans.Add(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã thêm nhà xuất bản.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.NhaXuatBans.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NhaXuatBan model)
        {
            if (id != model.NhaXuatBanId) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var exists = await _db.NhaXuatBans.AnyAsync(x => x.NhaXuatBanId == id);
            if (!exists) return NotFound();

            _db.NhaXuatBans.Update(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã cập nhật nhà xuất bản.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.NhaXuatBans.FindAsync(id);
            if (item == null) return NotFound();
            _db.NhaXuatBans.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
