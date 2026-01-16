using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
 
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var nxb = await _db.NhaXuatBans.FindAsync(id);
                if (nxb == null) return NotFound();

                // Detach: set DauSach.NhaXuatBanId = NULL
                await _db.DauSaches
                    .Where(ds => ds.NhaXuatBanId == id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(ds => ds.NhaXuatBanId, (int?)null));

                _db.NhaXuatBans.Remove(nxb);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["ok"] = "Đã xoá NXB. Các đầu sách liên quan đã được gán NhaXuatBan = NULL.";
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
