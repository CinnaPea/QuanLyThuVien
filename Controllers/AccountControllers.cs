using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Models; // đổi namespace cho đúng

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

        public IActionResult AccessDenied() => View();

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
