using WebApplication1.Models;

namespace WebApplication1.Services.Interfaces
{
    public interface IAuthService
    {
        TaiKhoan DangNhap(string tenDangNhap, string matKhau);
        void TaoTaiKhoan(TaiKhoan taiKhoan);
        void KhoaTaiKhoan(int taiKhoanId);
    }

}
