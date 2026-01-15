using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Models; 
namespace LibraryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly QuanLyThuVienContext _db;
        public AccountController(QuanLyThuVienContext db) => _db = db;

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string tenDangNhap, string matKhau, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            // Lưu ý: nếu DB bạn đang lưu plaintext thì đổi so sánh cho phù hợp.
            var passHash = Sha256(matKhau);

            var tk = await _db.TaiKhoans
                .Include(x => x.VaiTro)
                .FirstOrDefaultAsync(x => x.TenDangNhap == tenDangNhap);

            if (tk == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View();
            }

            // Nếu bạn đang lưu plaintext thì dùng: tk.MatKhauHash == matKhau
            if (!string.Equals(tk.MatKhauHash, passHash, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View();
            }

            var roleName = tk.VaiTro?.TenVaiTro ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, tk.TaiKhoanId.ToString()),
                new Claim(ClaimTypes.Name, tk.TenDangNhap),
                new Claim(ClaimTypes.Role, roleName),
            };

            // Nếu tài khoản có DocGiaId
            if (tk.DocGiaId.HasValue)
                claims.Add(new Claim("DocGiaId", tk.DocGiaId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            // Điều hướng
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Index", "Admin", new { area = "Admin" });

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ========== REGISTER ==========
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string tenDangNhap, string matKhau, string xacNhanMatKhau,
                                                 string hoTen, string? email, string? dienThoai, string? diaChi)
        {
            // basic validate
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau) || string.IsNullOrWhiteSpace(hoTen))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Tên đăng nhập, Mật khẩu và Họ tên.";
                return View();
            }

            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            // check username exists
            var exists = await _db.TaiKhoans.AnyAsync(x => x.TenDangNhap == tenDangNhap);
            if (exists)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                return View();
            }

            // 1) Create DocGia
            var docGia = new DocGium
            {
                HoTen = hoTen,
                Email = email,
                DienThoai = dienThoai,
                DiaChi = diaChi
            };

            _db.DocGia.Add(docGia);
            await _db.SaveChangesAsync(); // lấy DocGiaId

            // 2) Create TaiKhoan
            var tk = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = Sha256(matKhau),
                VaiTroId = 2, // User role
                TrangThai = true,
                DocGiaId = docGia.DocGiaId
            };

            _db.TaiKhoans.Add(tk);
            await _db.SaveChangesAsync();

            // auto login (session)
            HttpContext.Session.SetString("DOCGIA_ID", docGia.DocGiaId.ToString());
            HttpContext.Session.SetString("ROLE", "User");
            HttpContext.Session.SetString("USERNAME", tenDangNhap);

            TempData["SuccessMessage"] = "Đăng ký thành công! Bạn đã đăng nhập.";
            return RedirectToAction("Index", "Home");
        }
        public IActionResult AccessDenied() => View();

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
