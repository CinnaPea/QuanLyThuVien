
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TheLoaiController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public TheLoaiController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.TheLoais.OrderBy(x => x.TenTheLoai).ToListAsync());

        public IActionResult Create() => View(new TheLoai());

        [HttpPost]
        public async Task<IActionResult> Create(TheLoai model)
        {
            _db.TheLoais.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TheLoais.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TheLoai model)
        {
            _db.TheLoais.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.TheLoais.FindAsync(id);
            if (item == null) return NotFound();
            _db.TheLoais.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
