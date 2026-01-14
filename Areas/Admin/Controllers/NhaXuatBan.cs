using WebApplication1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace LibraryManagement.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NhaXuatBanController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public NhaXuatBanController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.NhaXuatBans.OrderBy(x => x.TenNxb).ToListAsync());

        public IActionResult Create() => View(new NhaXuatBan());

        [HttpPost]
        public async Task<IActionResult> Create(NhaXuatBan model)
        {
            _db.NhaXuatBans.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.NhaXuatBans.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(NhaXuatBan model)
        {
            _db.NhaXuatBans.Update(model);
            await _db.SaveChangesAsync();
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
