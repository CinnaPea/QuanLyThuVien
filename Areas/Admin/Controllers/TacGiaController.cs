using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
  
    public class TacGiaController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public TacGiaController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.TacGia.OrderBy(x => x.TacGiaId).ToListAsync());

        public IActionResult Create() => View(new TacGium());
        private static string NormalizeName(string? s)
        {
            s ??= "";
            s = s.Trim();
            s = Regex.Replace(s, @"\s+", " "); // collapse multiple spaces
            return s;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TacGium model)
        {
            model.TenTacGia = NormalizeName(model.TenTacGia);

            if (!ModelState.IsValid)
                return View(model);

            // Duplicate check
            var dup = await _db.TacGia.AnyAsync(x =>
                x.TenTacGia != null &&
                x.TenTacGia.ToLower() == model.TenTacGia.ToLower()
            );

            if (dup)
            {
                ModelState.AddModelError(nameof(TacGium.TenTacGia), "Tác giả này đã tồn tại.");
                return View(model);
            }

            _db.TacGia.Add(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã thêm tác giả.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TacGia.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TacGium model)
        {
            if (id != model.TacGiaId) return BadRequest();

            model.TenTacGia = NormalizeName(model.TenTacGia);

            if (!ModelState.IsValid)
                return View(model);

            // Duplicate check excluding current record
            var dup = await _db.TacGia.AnyAsync(x =>
                x.TacGiaId != model.TacGiaId &&
                x.TenTacGia != null &&
                x.TenTacGia.ToLower() == model.TenTacGia.ToLower()
            );

            if (dup)
            {
                ModelState.AddModelError(nameof(TacGium.TenTacGia), "Tác giả này đã tồn tại.");
                return View(model);
            }

            var exists = await _db.TacGia.AnyAsync(x => x.TacGiaId == id);
            if (!exists) return NotFound();

            _db.TacGia.Update(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã cập nhật tác giả.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var tg = await _db.TacGia.FindAsync(id);
                if (tg == null) return NotFound();

                // 1) Remove many-to-many links first (DauSach_TacGia)
                var links = await _db.DauSachTacGia
                    .Where(x => x.TacGiaId == id)
                    .ToListAsync();

                if (links.Count > 0)
                    _db.DauSachTacGia.RemoveRange(links);

                // 2) Now delete the author
                _db.TacGia.Remove(tg);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["ok"] = "Đã xoá tác giả (và gỡ liên kết với các đầu sách).";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                TempData["err"] = "Xoá thất bại do ràng buộc dữ liệu (FK). " + ex.GetBaseException().Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["err"] = ex.GetBaseException().Message;
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
