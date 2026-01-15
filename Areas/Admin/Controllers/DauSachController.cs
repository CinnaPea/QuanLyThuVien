using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
 
    public class DauSachController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public DauSachController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var list = await _db.DauSaches
                .Include(x => x.TheLoai)
                .Include(x => x.NhaXuatBan)
                .OrderBy(x => x.DauSachId)
                .ToListAsync();
            return View(list);
        }
        private void LoadDropDowns(DauSach? item = null)
        {
            ViewBag.TheLoaiId = new SelectList(
                _db.TheLoais.ToList(),
                "TheLoaiId",
                "TenTheLoai",
                item?.TheLoaiId
            );

            ViewBag.NhaXuatBanId = new SelectList(
                _db.NhaXuatBans.ToList(),
                "NhaXuatBanId",
                "TenNxb",          // ✅ FIX THIS (match your model property exactly)
                item?.NhaXuatBanId
            );
        }

        public IActionResult Create()
        {
            LoadDropDowns();
            return View(new DauSach());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DauSach model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropDowns(model);
                return View(model);
            }

            _db.DauSaches.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.DauSaches.FindAsync(id);
            if (item == null) return NotFound();

            LoadDropDowns(item);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DauSach model)
        {
            if (id != model.DauSachId) return BadRequest();

            if (!ModelState.IsValid)
            {
                LoadDropDowns(model);
                return View(model);
            }

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
