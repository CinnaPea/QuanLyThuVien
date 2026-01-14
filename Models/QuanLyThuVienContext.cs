using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public partial class QuanLyThuVienContext : DbContext
{
    public QuanLyThuVienContext()
    {
    }

    public QuanLyThuVienContext(DbContextOptions<QuanLyThuVienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CtPhieuMuon> CtPhieuMuons { get; set; }

    public virtual DbSet<CtPhieuTra> CtPhieuTras { get; set; }

    public virtual DbSet<DauSach> DauSaches { get; set; }

    public virtual DbSet<DauSachTacGium> DauSachTacGia { get; set; }

    public virtual DbSet<DocGium> DocGia { get; set; }

    public virtual DbSet<NhaXuatBan> NhaXuatBans { get; set; }

    public virtual DbSet<Phat> Phats { get; set; }

    public virtual DbSet<PhieuMuon> PhieuMuons { get; set; }

    public virtual DbSet<PhieuTra> PhieuTras { get; set; }

    public virtual DbSet<Sach> Saches { get; set; }

    public virtual DbSet<TacGium> TacGia { get; set; }

    public virtual DbSet<ThanhToanPhat> ThanhToanPhats { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=PEANUT\\SQLEXPRESS;Initial Catalog=QuanLyThuVien;Integrated Security=True;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<CtPhieuMuon>(entity =>
        {
            entity.HasKey(e => new { e.PhieuMuonId, e.SachId });

            entity.ToTable("CT_PhieuMuon");

            entity.Property(e => e.TinhTrangLucMuon).HasMaxLength(100);
            entity.Property(e => e.TinhTrangLucTra).HasMaxLength(100);

            entity.HasOne(d => d.PhieuMuon).WithMany(p => p.CtPhieuMuons)
                .HasForeignKey(d => d.PhieuMuonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPM_PhieuMuon");

            entity.HasOne(d => d.Sach).WithMany(p => p.CtPhieuMuons)
                .HasForeignKey(d => d.SachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPM_Sach");
        });

        modelBuilder.Entity<CtPhieuTra>(entity =>
        {
            entity.HasKey(e => new { e.PhieuTraId, e.SachId });

            entity.ToTable("CT_PhieuTra");

            entity.Property(e => e.TinhTrangLucTra).HasMaxLength(100);

            entity.HasOne(d => d.PhieuTra).WithMany(p => p.CtPhieuTras)
                .HasForeignKey(d => d.PhieuTraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPT_PhieuTra");

            entity.HasOne(d => d.Sach).WithMany(p => p.CtPhieuTras)
                .HasForeignKey(d => d.SachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CTPT_Sach");
        });

        modelBuilder.Entity<DauSach>(entity =>
        {
            entity.HasKey(e => e.DauSachId).HasName("PK__DauSach__93F51B8B40651502");

            entity.ToTable("DauSach");

            entity.HasIndex(e => e.Isbn, "UQ__DauSach__447D36EA39162874").IsUnique();

            entity.Property(e => e.Isbn)
                .HasMaxLength(20)
                .HasColumnName("ISBN");
            entity.Property(e => e.NgonNgu).HasMaxLength(50);
            entity.Property(e => e.TieuDe).HasMaxLength(255);

            entity.HasOne(d => d.NhaXuatBan).WithMany(p => p.DauSaches)
                .HasForeignKey(d => d.NhaXuatBanId)
                .HasConstraintName("FK_DauSach_NhaXuatBan");

            entity.HasOne(d => d.TheLoai).WithMany(p => p.DauSaches)
                .HasForeignKey(d => d.TheLoaiId)
                .HasConstraintName("FK_DauSach_TheLoai");
        });

        modelBuilder.Entity<DauSachTacGium>(entity =>
        {
            entity.HasKey(e => new { e.DauSachId, e.TacGiaId });

            entity.ToTable("DauSach_TacGia");

            entity.Property(e => e.VaiTro).HasMaxLength(50);

            entity.HasOne(d => d.DauSach).WithMany(p => p.DauSachTacGia)
                .HasForeignKey(d => d.DauSachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DSTG_DauSach");

            entity.HasOne(d => d.TacGia).WithMany(p => p.DauSachTacGia)
                .HasForeignKey(d => d.TacGiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DSTG_TacGia");
        });

        modelBuilder.Entity<DocGium>(entity =>
        {
            entity.HasKey(e => e.DocGiaId).HasName("PK__DocGia__E34FB53CB8ECFA18");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.DienThoai).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.NgayDangKy).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<NhaXuatBan>(entity =>
        {
            entity.HasKey(e => e.NhaXuatBanId).HasName("PK__NhaXuatB__E0587C0ED1135116");

            entity.ToTable("NhaXuatBan");

            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.DienThoai).HasMaxLength(20);
            entity.Property(e => e.TenNxb)
                .HasMaxLength(150)
                .HasColumnName("TenNXB");
        });

        modelBuilder.Entity<Phat>(entity =>
        {
            entity.HasKey(e => e.PhatId).HasName("PK__Phat__F2A5F66BEE7A7B09");

            entity.ToTable("Phat");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.LyDo).HasMaxLength(100);
            entity.Property(e => e.NgayLap).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.PhieuMuon).WithMany(p => p.Phats)
                .HasForeignKey(d => d.PhieuMuonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Phat_PhieuMuon");

            entity.HasOne(d => d.Sach).WithMany(p => p.Phats)
                .HasForeignKey(d => d.SachId)
                .HasConstraintName("FK_Phat_Sach");
        });

        modelBuilder.Entity<PhieuMuon>(entity =>
        {
            entity.HasKey(e => e.PhieuMuonId).HasName("PK__PhieuMuo__172705F19D7672D1");

            entity.ToTable("PhieuMuon");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayMuon).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đang mượn");

            entity.HasOne(d => d.DocGia).WithMany(p => p.PhieuMuons)
                .HasForeignKey(d => d.DocGiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhieuMuon_DocGia");
        });

        modelBuilder.Entity<PhieuTra>(entity =>
        {
            entity.HasKey(e => e.PhieuTraId).HasName("PK__PhieuTra__35C6280D5F4EA923");

            entity.ToTable("PhieuTra");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayTra).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.PhieuMuon).WithMany(p => p.PhieuTras)
                .HasForeignKey(d => d.PhieuMuonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhieuTra_PhieuMuon");
        });

        modelBuilder.Entity<Sach>(entity =>
        {
            entity.HasKey(e => e.SachId).HasName("PK__Sach__F3005E1A548AA6F7");

            entity.ToTable("Sach");

            entity.HasIndex(e => e.MaVach, "UQ__Sach__8BBF4A1C92D9ED94").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.MaVach).HasMaxLength(50);
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(30)
                .HasDefaultValue("Có sẵn");
            entity.Property(e => e.ViTriKe).HasMaxLength(100);

            entity.HasOne(d => d.DauSach).WithMany(p => p.Saches)
                .HasForeignKey(d => d.DauSachId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Sach_DauSach");
        });

        modelBuilder.Entity<TacGium>(entity =>
        {
            entity.HasKey(e => e.TacGiaId).HasName("PK__TacGia__E80231298F259433");

            entity.Property(e => e.TenTacGia).HasMaxLength(150);
        });

        modelBuilder.Entity<ThanhToanPhat>(entity =>
        {
            entity.HasKey(e => e.ThanhToanPhatId).HasName("PK__ThanhToa__64EA6A2C9CA4FFC4");

            entity.ToTable("ThanhToanPhat");

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayThanhToan).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Phat).WithMany(p => p.ThanhToanPhats)
                .HasForeignKey(d => d.PhatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhToanPhat_Phat");
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.TheLoaiId).HasName("PK__TheLoai__241C3B1A59FB06B8");

            entity.ToTable("TheLoai");

            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
