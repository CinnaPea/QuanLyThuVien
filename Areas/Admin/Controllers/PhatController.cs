using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class PhatController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public PhatController(QuanLyThuVienContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var list = await _db.Phats
                .OrderByDescending(x => x.PhatId)
                .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> Details(int id)
        {
            var phat = await _db.Phats
                .FirstOrDefaultAsync(x => x.PhatId == id);

            if (phat == null) return NotFound();

            var pays = await _db.ThanhToanPhats
                .Where(x => x.PhatId == id)
                .OrderByDescending(x => x.ThanhToanPhatId)
                .ToListAsync();

            ViewBag.Payments = pays;
            ViewBag.TotalPaid = pays.Sum(x => x.SoTien);

            return View(phat);
        }

        [HttpPost]
        public async Task<IActionResult> Pay(int phatId, decimal soTien)
        {
            if (soTien <= 0)
            {
                TempData["err"] = "Số tiền không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = phatId });
            }

            var phat = await _db.Phats.FindAsync(phatId);
            if (phat == null) return NotFound();

            // Ghi nhận thanh toán
            _db.ThanhToanPhats.Add(new ThanhToanPhat
            {
                PhatId = phatId,
                SoTien = soTien,
                NgayThanhToan = DateTime.Now,
                GhiChu = null
            });

            await _db.SaveChangesAsync();

            // Tính tổng đã trả
            var tongDaTra = await _db.ThanhToanPhats
                .Where(x => x.PhatId == phatId)
                .SumAsync(x => x.SoTien);

            // Đánh dấu đã thanh toán nếu đủ tiền
            if (tongDaTra >= phat.SoTien)
                phat.DaThanhToan = true;

            await _db.SaveChangesAsync();

            TempData["ok"] = "Đã ghi nhận thanh toán.";
            return RedirectToAction(nameof(Details), new { id = phatId });
        }
    }
}
