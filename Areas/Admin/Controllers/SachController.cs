using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using System.Security.Cryptography;
using WebApplication1.VMs;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
   
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

            var entity = await _db.Saches.FindAsync(id);
            if (entity == null) return NotFound();

            // cập nhật từng field
            entity.DauSachId = model.DauSachId;
            entity.SachId = model.SachId;
            entity.TinhTrang = model.TinhTrang;   // ⭐ QUAN TRỌNG
            entity.GhiChu = model.GhiChu;

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

        [HttpGet]
        public async Task<IActionResult> CreateBatch(int dauSachId)
        {
            var ds = await _db.DauSaches
                .AsNoTracking()
                .Where(x => x.DauSachId == dauSachId)
                .Select(x => new { x.DauSachId, x.TieuDe })
                .FirstOrDefaultAsync();

            if (ds == null) return NotFound();

            var vm = new SachBatchVM
            {
                DauSachId = ds.DauSachId,
                DauSachTieuDe = ds.TieuDe,
                SoLuong = 1,
                NgayNhap = DateOnly.FromDateTime(DateTime.Today),
                TinhTrang = "Con"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBatch(SachBatchVM vm)
        {
            // guardrails
            if (!ModelState.IsValid) return View(vm);

            var exists = await _db.DauSaches.AnyAsync(x => x.DauSachId == vm.DauSachId);
            if (!exists)
            {
                ModelState.AddModelError("", "Đầu sách không tồn tại.");
                return View(vm);
            }

            // Generate + insert
            for (int i = 0; i < vm.SoLuong; i++)
            {
                var s = new Sach
                {
                    DauSachId = vm.DauSachId,
                    ViTriKe = vm.ViTriKe,
                    NgayNhap = vm.NgayNhap,
                    TinhTrang = string.IsNullOrWhiteSpace(vm.TinhTrang) ? "Con" : vm.TinhTrang,
                    GhiChu = vm.GhiChu,
                    MaVach = await GenerateUniqueMaVachAsync(vm.DauSachId)
                };

                _db.Saches.Add(s);
            }

            await _db.SaveChangesAsync();
            TempData["ok"] = $"Đã tạo {vm.SoLuong} sách bản sao.";
            return RedirectToAction("Index", "Sach", new { area = "Admin" });
        }

        private async Task<string> GenerateUniqueMaVachAsync(int dauSachId)
        {
            // Format: DS00012-20260115-AB12CD (short, readable, sortable-ish)
            while (true)
            {
                var suffix = RandomHex(6);
                var code = $"DS{dauSachId:D5}-{DateTime.UtcNow:yyyyMMdd}-{suffix}";

                var dup = await _db.Saches.AnyAsync(x => x.MaVach == code);
                if (!dup) return code;
            }
        }

        private static string RandomHex(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes((length + 1) / 2);
            var hex = Convert.ToHexString(bytes);
            return hex.Substring(0, length);
        }
    }
}
