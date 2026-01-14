using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly QuanLyThuVienContext _context;

        public AuthService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public TaiKhoan DangNhap(string tenDangNhap, string matKhau)
        {
            var hash = Hash(matKhau);

            return _context.TaiKhoans
                .Include(t => t.VaiTro)
                .FirstOrDefault(t =>
                    t.TenDangNhap == tenDangNhap &&
                    t.MatKhauHash == hash &&
                    t.TrangThai);
        }

        public void TaoTaiKhoan(TaiKhoan taiKhoan)
        {
            taiKhoan.MatKhauHash = Hash(taiKhoan.MatKhauHash);
            taiKhoan.TrangThai = true;

            _context.TaiKhoans.Add(taiKhoan);
            _context.SaveChanges();
        }

        public void KhoaTaiKhoan(int taiKhoanId)
        {
            var tk = _context.TaiKhoans.Find(taiKhoanId);
            tk.TrangThai = false;
            _context.SaveChanges();
        }

        private string Hash(string raw)
        {
            // BCrypt / PBKDF2 / ASP.NET Identity hashing
            return raw; // placeholder
        }
    }

}
