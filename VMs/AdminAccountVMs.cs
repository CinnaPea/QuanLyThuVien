using System.ComponentModel.DataAnnotations;

namespace WebApplication1.VMs
{
    public class AccountListItemVM
    {
        public int TaiKhoanId { get; set; }
        public string TenDangNhap { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public int VaiTroId { get; set; }

        public int DocGiaId { get; set; }
        public string HoTen { get; set; } = "";
        public string? Email { get; set; }
        public string? DienThoai { get; set; }

        public bool TrangThai { get; set; }
    }

    public class AccountCreateVM
    {
        // TaiKhoan
        [Required, StringLength(50)]
        public string TenDangNhap { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 4)]
        public string MatKhau { get; set; } = "";

        [Required]
        public int VaiTroId { get; set; }

        public bool TrangThai { get; set; } = true;

        // DocGia
        [Required, StringLength(150)]
        public string HoTen { get; set; } = "";

        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? DienThoai { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }
    }

    public class AccountEditVM
    {
        public int TaiKhoanId { get; set; }
        public int DocGiaId { get; set; }

        [Required, StringLength(50)]
        public string TenDangNhap { get; set; } = "";

        [Required]
        public int VaiTroId { get; set; }

        public bool TrangThai { get; set; }

        // Optional reset password
        [StringLength(100, MinimumLength = 4)]
        public string? MatKhauMoi { get; set; }

        // DocGia
        [Required, StringLength(150)]
        public string HoTen { get; set; } = "";

        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? DienThoai { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }
    }
}
