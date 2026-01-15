using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.VMs;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccountController : Controller
    {
        private readonly QuanLyThuVienContext _db; // đổi tên theo DbContext của bạn

        public AccountController(QuanLyThuVienContext db)
        {
            _db = db;
        }

        private async Task LoadVaiTroSelectAsync(int? selectedId = null)
        {
            var roles = await _db.VaiTros
                .OrderBy(x => x.TenVaiTro)
                .Select(x => new { x.VaiTroId, x.TenVaiTro })
                .ToListAsync();

            ViewBag.VaiTroList = new SelectList(roles, "VaiTroId", "TenVaiTro", selectedId);
        }

        // ===== INDEX (example safe nullable filtering) =====
        public async Task<IActionResult> Index(string? q, int? vaiTroId, bool? trangThai)
        {
            var query = _db.TaiKhoans
                .AsNoTracking()
                .Include(x => x.VaiTro)
                .Include(x => x.DocGia)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                    x.TenDangNhap.Contains(q) ||
                    (x.DocGia != null && x.DocGia.HoTen.Contains(q)) ||
                    (x.DocGia != null && x.DocGia.Email != null && x.DocGia.Email.Contains(q)));
            }

            if (vaiTroId.HasValue)
                query = query.Where(x => x.VaiTroId == vaiTroId.Value);

            if (trangThai.HasValue)
                query = query.Where(x => x.TrangThai == trangThai.Value);

            var data = await query
                .OrderByDescending(x => x.TaiKhoanId)
                .Select(x => new AccountListItemVM
                {
                    TaiKhoanId = x.TaiKhoanId,
                    TenDangNhap = x.TenDangNhap,
                    VaiTroId = x.VaiTroId,
                    VaiTro = x.VaiTro != null ? x.VaiTro.TenVaiTro : "",
                    DocGiaId = x.DocGiaId ?? 0,
                    HoTen = x.DocGia != null ? x.DocGia.HoTen : "",
                    Email = x.DocGia != null ? x.DocGia.Email : null,
                    DienThoai = x.DocGia != null ? x.DocGia.DienThoai : null,
                    TrangThai = x.TrangThai
                })
                .ToListAsync();
            ViewBag.VaiTros = await _db.VaiTros
            .OrderBy(v => v.VaiTroId) // or OrderBy(v => v.TenVaiTro)
            .ToListAsync();

            ViewBag.SelectedVaiTroId = vaiTroId;
            ViewBag.Q = q;
            ViewBag.TrangThai = trangThai;

            return View(data);
        }

        // ===== DETAILS (use AccountEditVM as "details dto") =====
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.TaiKhoans
                .Include(x => x.VaiTro)
                .Include(x => x.DocGia)
                .FirstOrDefaultAsync(x => x.TaiKhoanId == id);

            if (item == null) return NotFound();
            return View(item); // <-- TaiKhoan
        }


        // ===== CREATE =====
        public async Task<IActionResult> Create()
        {
            await LoadVaiTroSelectAsync();
            return View(new AccountCreateVM { TrangThai = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadVaiTroSelectAsync(vm.VaiTroId);
                return View(vm);
            }

            // Unique username check
            var exists = await _db.TaiKhoans.AnyAsync(x => x.TenDangNhap == vm.TenDangNhap);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập đã tồn tại.");
                await LoadVaiTroSelectAsync(vm.VaiTroId);
                return View(vm);
            }

            var docGia = new DocGium
            {
                HoTen = vm.HoTen,
                Email = vm.Email,
                DienThoai = vm.DienThoai,
                DiaChi = vm.DiaChi,
                NgayDangKy = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = true
            };

            _db.DocGia.Add(docGia);
            await _db.SaveChangesAsync();

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = vm.TenDangNhap,
                MatKhauHash = HashPassword(vm.MatKhau), // TODO: thay bằng hàm hash thật của bạn
                VaiTroId = vm.VaiTroId,
                DocGiaId = docGia.DocGiaId,
                TrangThai = vm.TrangThai
            };

            _db.TaiKhoans.Add(taiKhoan);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ===== EDIT =====
        public async Task<IActionResult> Edit(int id)
        {
            var acc = await _db.TaiKhoans
                .Include(x => x.DocGia)
                .FirstOrDefaultAsync(x => x.TaiKhoanId == id);

            if (acc == null) return NotFound();

            var vm = new AccountEditVM
            {
                TaiKhoanId = acc.TaiKhoanId,
                DocGiaId = (int)acc.DocGiaId!,
                TenDangNhap = acc.TenDangNhap,
                VaiTroId = acc.VaiTroId,
                TrangThai = acc.TrangThai,
                HoTen = acc.DocGia?.HoTen ?? "",
                Email = acc.DocGia?.Email,
                DienThoai = acc.DocGia?.DienThoai,
                DiaChi = acc.DocGia?.DiaChi
            };

            await LoadVaiTroSelectAsync(vm.VaiTroId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccountEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadVaiTroSelectAsync(vm.VaiTroId);
                return View(vm);
            }

            var acc = await _db.TaiKhoans
                .Include(x => x.DocGia)
                .FirstOrDefaultAsync(x => x.TaiKhoanId == vm.TaiKhoanId);

            if (acc == null) return NotFound();

            // Unique username check (exclude self)
            var exists = await _db.TaiKhoans.AnyAsync(x => x.TenDangNhap == vm.TenDangNhap && x.TaiKhoanId != vm.TaiKhoanId);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.TenDangNhap), "Tên đăng nhập đã tồn tại.");
                await LoadVaiTroSelectAsync(vm.VaiTroId);
                return View(vm);
            }

            acc.TenDangNhap = vm.TenDangNhap;
            acc.VaiTroId = vm.VaiTroId;
            acc.TrangThai = vm.TrangThai;

            if (!string.IsNullOrWhiteSpace(vm.MatKhauMoi))
            {
                acc.MatKhauHash = HashPassword(vm.MatKhauMoi);
            }

            if (acc.DocGia != null)
            {
                acc.DocGia.HoTen = vm.HoTen;
                acc.DocGia.Email = vm.Email;
                acc.DocGia.DienThoai = vm.DienThoai;
                acc.DocGia.DiaChi = vm.DiaChi;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = vm.TaiKhoanId });
        }

        // ===== Replace this with your real hashing =====
        private string HashPassword(string raw)
        {
            // TODO: replace. (BCrypt / Identity hasher / your existing util)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
        }
    }
}
