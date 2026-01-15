using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Models;

namespace LibraryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public AccountController(QuanLyThuVienContext db) => _db = db;

        // ================= LOGIN =================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau)
        {
            var passHash = Sha256(matKhau);

            var tk = await _db.TaiKhoans.Include(x=> x.DocGia)
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

            if (tk == null ||
    !string.Equals(tk.MatKhauHash, passHash, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View();
            }

            var role = tk.VaiTro?.TenVaiTro ?? "User";

            // ===== LƯU SESSION =====
            HttpContext.Session.SetInt32("TAIKHOAN_ID", tk.TaiKhoanId);
            HttpContext.Session.SetString("USERNAME", tk.TenDangNhap);
            HttpContext.Session.SetString("ROLE", role);

            if (tk.DocGiaId.HasValue)
                HttpContext.Session.SetInt32("DOCGIA_ID", tk.DocGiaId.Value);

            // ===== ĐIỀU HƯỚNG =====
            if (role == "Admin")
                return RedirectToAction("Index", "Admin", new { area = "Admin" });

            return RedirectToAction("Index", "Home");
        }


        // ================= LOGOUT =================
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ================= REGISTER =================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(
            string tenDangNhap,
            string matKhau,
            string xacNhanMatKhau,
            string hoTen,
            string? email,
            string? dienThoai,
            string? diaChi)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) ||
                string.IsNullOrWhiteSpace(matKhau) ||
                string.IsNullOrWhiteSpace(hoTen))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            var exists = await _db.TaiKhoans.AnyAsync(x => x.TenDangNhap == tenDangNhap);
            if (exists)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            // 1. Tạo Độc Giả
            var docGia = new DocGium
            {
                HoTen = hoTen,
                Email = email,
                DienThoai = dienThoai,
                DiaChi = diaChi,
                TrangThai = true,
                NgayDangKy = DateOnly.FromDateTime(DateTime.Now)
            };

            _db.DocGia.Add(docGia);
            await _db.SaveChangesAsync();

            // 2. Tạo Tài Khoản (User)
            var tk = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = Sha256(matKhau),
                VaiTroId = 2, // USER
                TrangThai = true,
                DocGiaId = docGia.DocGiaId
            };

            _db.TaiKhoans.Add(tk);
            await _db.SaveChangesAsync();

            // 3. Auto login bằng SESSION
            HttpContext.Session.SetInt32("TAIKHOAN_ID", tk.TaiKhoanId);
            HttpContext.Session.SetString("USERNAME", tenDangNhap);
            HttpContext.Session.SetString("ROLE", "User");
            HttpContext.Session.SetInt32("DOCGIA_ID", docGia.DocGiaId);

            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("Index", "Home");
        }

        // ================= HASH =================
        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
