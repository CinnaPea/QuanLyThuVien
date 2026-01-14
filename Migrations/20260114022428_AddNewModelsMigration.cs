using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AddNewModelsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocGia",
                columns: table => new
                {
                    DocGiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NgayDangKy = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(CONVERT([date],getdate()))"),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DocGia__E34FB53CB8ECFA18", x => x.DocGiaId);
                });

            migrationBuilder.CreateTable(
                name: "NhaXuatBan",
                columns: table => new
                {
                    NhaXuatBanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNXB = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__NhaXuatB__E0587C0ED1135116", x => x.NhaXuatBanId);
                });

            migrationBuilder.CreateTable(
                name: "TacGia",
                columns: table => new
                {
                    TacGiaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTacGia = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TacGia__E80231298F259433", x => x.TacGiaId);
                });

            migrationBuilder.CreateTable(
                name: "TheLoai",
                columns: table => new
                {
                    TheLoaiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTheLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TheLoai__241C3B1A59FB06B8", x => x.TheLoaiId);
                });

            migrationBuilder.CreateTable(
                name: "PhieuMuon",
                columns: table => new
                {
                    PhieuMuonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocGiaId = table.Column<int>(type: "int", nullable: false),
                    NgayMuon = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    HanTra = table.Column<DateOnly>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Đang mượn"),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhieuMuo__172705F19D7672D1", x => x.PhieuMuonId);
                    table.ForeignKey(
                        name: "FK_PhieuMuon_DocGia",
                        column: x => x.DocGiaId,
                        principalTable: "DocGia",
                        principalColumn: "DocGiaId");
                });

            migrationBuilder.CreateTable(
                name: "DauSach",
                columns: table => new
                {
                    DauSachId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TieuDe = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NamXuatBan = table.Column<int>(type: "int", nullable: true),
                    NgonNgu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheLoaiId = table.Column<int>(type: "int", nullable: true),
                    NhaXuatBanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DauSach__93F51B8B40651502", x => x.DauSachId);
                    table.ForeignKey(
                        name: "FK_DauSach_NhaXuatBan",
                        column: x => x.NhaXuatBanId,
                        principalTable: "NhaXuatBan",
                        principalColumn: "NhaXuatBanId");
                    table.ForeignKey(
                        name: "FK_DauSach_TheLoai",
                        column: x => x.TheLoaiId,
                        principalTable: "TheLoai",
                        principalColumn: "TheLoaiId");
                });

            migrationBuilder.CreateTable(
                name: "PhieuTra",
                columns: table => new
                {
                    PhieuTraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuMuonId = table.Column<int>(type: "int", nullable: false),
                    NgayTra = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PhieuTra__35C6280D5F4EA923", x => x.PhieuTraId);
                    table.ForeignKey(
                        name: "FK_PhieuTra_PhieuMuon",
                        column: x => x.PhieuMuonId,
                        principalTable: "PhieuMuon",
                        principalColumn: "PhieuMuonId");
                });

            migrationBuilder.CreateTable(
                name: "DauSach_TacGia",
                columns: table => new
                {
                    DauSachId = table.Column<int>(type: "int", nullable: false),
                    TacGiaId = table.Column<int>(type: "int", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DauSach_TacGia", x => new { x.DauSachId, x.TacGiaId });
                    table.ForeignKey(
                        name: "FK_DSTG_DauSach",
                        column: x => x.DauSachId,
                        principalTable: "DauSach",
                        principalColumn: "DauSachId");
                    table.ForeignKey(
                        name: "FK_DSTG_TacGia",
                        column: x => x.TacGiaId,
                        principalTable: "TacGia",
                        principalColumn: "TacGiaId");
                });

            migrationBuilder.CreateTable(
                name: "Sach",
                columns: table => new
                {
                    SachId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DauSachId = table.Column<int>(type: "int", nullable: false),
                    MaVach = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ViTriKe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NgayNhap = table.Column<DateOnly>(type: "date", nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Có sẵn"),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sach__F3005E1A548AA6F7", x => x.SachId);
                    table.ForeignKey(
                        name: "FK_Sach_DauSach",
                        column: x => x.DauSachId,
                        principalTable: "DauSach",
                        principalColumn: "DauSachId");
                });

            migrationBuilder.CreateTable(
                name: "CT_PhieuMuon",
                columns: table => new
                {
                    PhieuMuonId = table.Column<int>(type: "int", nullable: false),
                    SachId = table.Column<int>(type: "int", nullable: false),
                    NgayTraThucTe = table.Column<DateOnly>(type: "date", nullable: true),
                    TinhTrangLucMuon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TinhTrangLucTra = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_PhieuMuon", x => new { x.PhieuMuonId, x.SachId });
                    table.ForeignKey(
                        name: "FK_CTPM_PhieuMuon",
                        column: x => x.PhieuMuonId,
                        principalTable: "PhieuMuon",
                        principalColumn: "PhieuMuonId");
                    table.ForeignKey(
                        name: "FK_CTPM_Sach",
                        column: x => x.SachId,
                        principalTable: "Sach",
                        principalColumn: "SachId");
                });

            migrationBuilder.CreateTable(
                name: "CT_PhieuTra",
                columns: table => new
                {
                    PhieuTraId = table.Column<int>(type: "int", nullable: false),
                    SachId = table.Column<int>(type: "int", nullable: false),
                    TinhTrangLucTra = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_PhieuTra", x => new { x.PhieuTraId, x.SachId });
                    table.ForeignKey(
                        name: "FK_CTPT_PhieuTra",
                        column: x => x.PhieuTraId,
                        principalTable: "PhieuTra",
                        principalColumn: "PhieuTraId");
                    table.ForeignKey(
                        name: "FK_CTPT_Sach",
                        column: x => x.SachId,
                        principalTable: "Sach",
                        principalColumn: "SachId");
                });

            migrationBuilder.CreateTable(
                name: "Phat",
                columns: table => new
                {
                    PhatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuMuonId = table.Column<int>(type: "int", nullable: false),
                    SachId = table.Column<int>(type: "int", nullable: true),
                    LyDo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DaThanhToan = table.Column<bool>(type: "bit", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Phat__F2A5F66BEE7A7B09", x => x.PhatId);
                    table.ForeignKey(
                        name: "FK_Phat_PhieuMuon",
                        column: x => x.PhieuMuonId,
                        principalTable: "PhieuMuon",
                        principalColumn: "PhieuMuonId");
                    table.ForeignKey(
                        name: "FK_Phat_Sach",
                        column: x => x.SachId,
                        principalTable: "Sach",
                        principalColumn: "SachId");
                });

            migrationBuilder.CreateTable(
                name: "ThanhToanPhat",
                columns: table => new
                {
                    ThanhToanPhatId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhatId = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ThanhToa__64EA6A2C9CA4FFC4", x => x.ThanhToanPhatId);
                    table.ForeignKey(
                        name: "FK_ThanhToanPhat_Phat",
                        column: x => x.PhatId,
                        principalTable: "Phat",
                        principalColumn: "PhatId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CT_PhieuMuon_SachId",
                table: "CT_PhieuMuon",
                column: "SachId");

            migrationBuilder.CreateIndex(
                name: "IX_CT_PhieuTra_SachId",
                table: "CT_PhieuTra",
                column: "SachId");

            migrationBuilder.CreateIndex(
                name: "IX_DauSach_NhaXuatBanId",
                table: "DauSach",
                column: "NhaXuatBanId");

            migrationBuilder.CreateIndex(
                name: "IX_DauSach_TheLoaiId",
                table: "DauSach",
                column: "TheLoaiId");

            migrationBuilder.CreateIndex(
                name: "UQ__DauSach__447D36EA39162874",
                table: "DauSach",
                column: "ISBN",
                unique: true,
                filter: "[ISBN] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DauSach_TacGia_TacGiaId",
                table: "DauSach_TacGia",
                column: "TacGiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Phat_PhieuMuonId",
                table: "Phat",
                column: "PhieuMuonId");

            migrationBuilder.CreateIndex(
                name: "IX_Phat_SachId",
                table: "Phat",
                column: "SachId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuMuon_DocGiaId",
                table: "PhieuMuon",
                column: "DocGiaId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuTra_PhieuMuonId",
                table: "PhieuTra",
                column: "PhieuMuonId");

            migrationBuilder.CreateIndex(
                name: "IX_Sach_DauSachId",
                table: "Sach",
                column: "DauSachId");

            migrationBuilder.CreateIndex(
                name: "UQ__Sach__8BBF4A1C92D9ED94",
                table: "Sach",
                column: "MaVach",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanPhat_PhatId",
                table: "ThanhToanPhat",
                column: "PhatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CT_PhieuMuon");

            migrationBuilder.DropTable(
                name: "CT_PhieuTra");

            migrationBuilder.DropTable(
                name: "DauSach_TacGia");

            migrationBuilder.DropTable(
                name: "ThanhToanPhat");

            migrationBuilder.DropTable(
                name: "PhieuTra");

            migrationBuilder.DropTable(
                name: "TacGia");

            migrationBuilder.DropTable(
                name: "Phat");

            migrationBuilder.DropTable(
                name: "PhieuMuon");

            migrationBuilder.DropTable(
                name: "Sach");

            migrationBuilder.DropTable(
                name: "DocGia");

            migrationBuilder.DropTable(
                name: "DauSach");

            migrationBuilder.DropTable(
                name: "NhaXuatBan");

            migrationBuilder.DropTable(
                name: "TheLoai");
        }
    }
}
