using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
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
                .OrderBy(x => x.SachId)
                .ToListAsync();
            return View(list);
        }
        private void LoadDauSachDropdown(int? selectedId = null)
        {
            ViewBag.DauSachId = new SelectList(
                _db.DauSaches.ToList(),
                "DauSachId",
                "TieuDe",
                selectedId
            );
        }

        public IActionResult Create()
        {
            LoadDauSachDropdown();
            return View(new Sach
            {
                TinhTrang = "Con",
                NgayNhap = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sach model)
        {
            if (!ModelState.IsValid)
            {
                LoadDauSachDropdown(model.DauSachId);
                return View(model);
            }

            _db.Saches.Add(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã thêm sách bản sao.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Saches.FindAsync(id);
            if (item == null) return NotFound();

            LoadDauSachDropdown(item.DauSachId);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sach model)
        {
            if (id != model.SachId) return BadRequest();

            if (!ModelState.IsValid)
            {
                LoadDauSachDropdown(model.DauSachId);
                return View(model);
            }

            var exists = await _db.Saches.AnyAsync(x => x.SachId == id);
            if (!exists) return NotFound();

            _db.Saches.Update(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã cập nhật sách bản sao.";
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
