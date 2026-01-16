
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WebApplication1.Models;
namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class TheLoaiController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public TheLoaiController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
            => View(await _db.TheLoais.OrderBy(x => x.TheLoaiId).ToListAsync());

        public IActionResult Create() => View(new TheLoai());
        private static string NormalizeName(string? s)
        {
            s ??= "";
            s = s.Trim();
            s = Regex.Replace(s, @"\s+", " ");
            return s;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TheLoai model)
        {
            model.TenTheLoai = NormalizeName(model.TenTheLoai);

            if (!ModelState.IsValid)
                return View(model);

            var dup = await _db.TheLoais.AnyAsync(x =>
                x.TenTheLoai != null &&
                x.TenTheLoai.ToLower() == model.TenTheLoai.ToLower()
            );

            if (dup)
            {
                ModelState.AddModelError(nameof(TheLoai.TenTheLoai), "Thể loại này đã tồn tại.");
                return View(model);
            }

            _db.TheLoais.Add(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã thêm thể loại.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.TheLoais.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TheLoai model)
        {
            if (id != model.TheLoaiId) return BadRequest();

            model.TenTheLoai = NormalizeName(model.TenTheLoai);

            if (!ModelState.IsValid)
                return View(model);

            var dup = await _db.TheLoais.AnyAsync(x =>
                x.TheLoaiId != model.TheLoaiId &&
                x.TenTheLoai != null &&
                x.TenTheLoai.ToLower() == model.TenTheLoai.ToLower()
            );

            if (dup)
            {
                ModelState.AddModelError(nameof(TheLoai.TenTheLoai), "Thể loại này đã tồn tại.");
                return View(model);
            }

            var exists = await _db.TheLoais.AnyAsync(x => x.TheLoaiId == id);
            if (!exists) return NotFound();

            _db.TheLoais.Update(model);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Đã cập nhật thể loại.";
            return RedirectToAction(nameof(Index));
        }

        //[HttpPost]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var item = await _db.TheLoais.FindAsync(id);
        //    if (item == null) return NotFound();
        //    _db.TheLoais.Remove(item);
        //    await _db.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var theLoai = await _db.TheLoais.FindAsync(id);
                if (theLoai == null) return NotFound();

                // Detach: set DauSach.TheLoaiId = NULL
                await _db.DauSaches
                    .Where(ds => ds.TheLoaiId == id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(ds => ds.TheLoaiId, (int?)null));

                _db.TheLoais.Remove(theLoai);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["ok"] = "Đã xoá thể loại. Các đầu sách liên quan đã được gán TheLoai = NULL.";
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
