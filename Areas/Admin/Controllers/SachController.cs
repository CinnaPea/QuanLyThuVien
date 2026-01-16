using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Saches.FindAsync(id);
            if (item == null) return NotFound();

            LoadDauSachDropdown(item.DauSachId);
            return View(item);
        }

        [HttpPost("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sach model)
        {
            if (id != model.SachId) return BadRequest();

            // parse DateOnly từ form (giữ như bạn đang làm)
            DateOnly? parsedNgayNhap = null;
            if (Request.Form.TryGetValue("NgayNhap", out var ngayNhapRaw) &&
                !string.IsNullOrWhiteSpace(ngayNhapRaw))
            {
                if (DateOnly.TryParse(ngayNhapRaw.ToString(), out var d))
                    parsedNgayNhap = d;
                else
                    ModelState.AddModelError(nameof(Sach.NgayNhap), "Ngày nhập không hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                TempData["err"] = "Form có lỗi. Hãy kiểm tra lại các trường nhập.";
                LoadDauSachDropdown(model.DauSachId);
                return View(model);
            }

            var entity = await _db.Saches.FindAsync(id);
            if (entity == null) return NotFound();

            entity.DauSachId = model.DauSachId;
            entity.MaVach = model.MaVach?.Trim();     // khuyên dùng: không fallback về entity cũ
            entity.ViTriKe = model.ViTriKe?.Trim();
            entity.NgayNhap = parsedNgayNhap;
            entity.TinhTrang = model.TinhTrang;
            entity.GhiChu = model.GhiChu?.Trim();

            var affected = await _db.SaveChangesAsync();

            TempData["ok"] = affected > 0
                ? "Đã cập nhật sách bản sao."
                : "Không có thay đổi nào được ghi nhận.";

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // id = SachId
            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                var sach = await _db.Saches.FindAsync(id);
                if (sach == null) return NotFound();

                // 1) Phat -> ThanhToanPhat (FK_ThanhToanPhat_Phat)
                var phatIds = await _db.Phats
                    .Where(p => p.SachId == id)
                    .Select(p => p.PhatId)
                    .ToListAsync();

                if (phatIds.Count > 0)
                {
                    var thanhToans = await _db.ThanhToanPhats
                        .Where(t => phatIds.Contains(t.PhatId))
                        .ToListAsync();

                    if (thanhToans.Count > 0)
                        _db.ThanhToanPhats.RemoveRange(thanhToans);

                    var phats = await _db.Phats
                        .Where(p => p.SachId == id)
                        .ToListAsync();

                    if (phats.Count > 0)
                        _db.Phats.RemoveRange(phats);
                }

                // 2) CT_PhieuMuon (FK_CTPM_Sach)
                var ctpm = await _db.CtPhieuMuons
                    .Where(x => x.SachId == id)
                    .ToListAsync();

                if (ctpm.Count > 0)
                    _db.CtPhieuMuons.RemoveRange(ctpm);

                // 3) CT_PhieuTra (FK_CTPT_Sach)
                var ctpt = await _db.CtPhieuTras
                    .Where(x => x.SachId == id)
                    .ToListAsync();

                if (ctpt.Count > 0)
                    _db.CtPhieuTras.RemoveRange(ctpt);

                // 4) Finally delete Sach
                _db.Saches.Remove(sach);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                TempData["ok"] = "Đã xoá sách bản sao (và toàn bộ dữ liệu liên quan).";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync();
                TempData["err"] = ex.GetBaseException().Message; // show real FK if any remains
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                TempData["err"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
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
