using System;
using System.Collections.Generic;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Data;

public partial class FptdrinkContext : DbContext
{
    public FptdrinkContext()
    {
    }

    public FptdrinkContext(DbContextOptions<FptdrinkContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<ChucNangQuyen> ChucNangQuyens { get; set; }

    public virtual DbSet<ChucVu> ChucVus { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<GioHangTam> GioHangTams { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<MigrationHistory> MigrationHistories { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhanQuyen> PhanQuyens { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ThongKe> ThongKes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.Category");

            entity.ToTable("Category");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Alias).HasMaxLength(150);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.SeoDescription).HasMaxLength(250);
            entity.Property(e => e.SeoKeywords).HasMaxLength(150);
            entity.Property(e => e.SeoTitle).HasMaxLength(150);
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.ChiTietHoaDon");

            entity.ToTable("ChiTietHoaDon");

            entity.HasIndex(e => e.OrderId, "IX_OrderID");

            entity.HasIndex(e => e.ProductId, "IX_ProductID");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("OrderID");
            entity.Property(e => e.ProductId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ProductID");

            entity.HasOne(d => d.Order).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_dbo.ChiTietHoaDon_dbo.HoaDon_OrderID");

            entity.HasOne(d => d.Product).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_dbo.ChiTietHoaDon_dbo.Product_ProductID");
        });

        modelBuilder.Entity<ChucNangQuyen>(entity =>
        {
            entity.HasKey(e => e.MaChucNang).HasName("PK_dbo.ChucNangQuyen");

            entity.ToTable("ChucNangQuyen");

            entity.Property(e => e.MaChucNang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.TenChucNang).HasMaxLength(500);
        });

        modelBuilder.Entity<ChucVu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.ChucVu");

            entity.ToTable("ChucVu");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.TenChucVu).HasMaxLength(500);
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.GioHang");

            entity.ToTable("GioHang");

            entity.HasIndex(e => e.IdKhachHang, "IX_GioHang_KhachHang");

            entity.HasIndex(e => e.MaSanPham, "IX_GioHang_Product");

            entity.HasIndex(e => new { e.IdKhachHang, e.MaSanPham }, "IX_GioHang_Unique").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.IdKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID_KhachHang");
            entity.Property(e => e.MaSanPham)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

            entity.HasOne(d => d.IdKhachHangNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.IdKhachHang)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GioHang_KhachHang");

            entity.HasOne(d => d.MaSanPhamNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaSanPham)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GioHang_Product");
        });

        modelBuilder.Entity<GioHangTam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.GioHangTam");

            entity.ToTable("GioHangTam");

            entity.HasIndex(e => e.CartId, "IX_GioHangTam_CartId");

            entity.HasIndex(e => e.ExpiryDate, "IX_GioHangTam_ExpiryDate");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CartId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.MaSanPham)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.MaSanPhamNavigation).WithMany(p => p.GioHangTams)
                .HasForeignKey(d => d.MaSanPham)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_GioHangTam_Product");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK_dbo.HoaDon");

            entity.ToTable("HoaDon");

            entity.Property(e => e.MaHoaDon)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Cccd)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasDefaultValue("")
                .HasColumnName("CCCD");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DiaChi).HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IdKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID_KhachHang");
            entity.Property(e => e.IdNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID_NhanVien");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai).IsUnicode(false);
            entity.Property(e => e.TenKhachHang).HasMaxLength(500);

            entity.HasOne(d => d.IdNhanVienNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.IdNhanVien)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_HoaDon_NhanVien");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.IdKhachHang)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_HoaDon_KhachHang");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.KhachHang");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.TenDangNhap, "IX_KhachHang_TenDangNhap").IsUnique();

            entity.HasIndex(e => e.Email, "IX_KhachHang_Email").IsUnique();

            entity.HasIndex(e => e.SoDienThoai, "IX_KhachHang_SoDienThoai");

            entity.HasIndex(e => e.GoogleId, "IX_KhachHang_GoogleId")
                .IsUnique()
                .HasFilter("([GoogleId] IS NOT NULL)");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.Cccd)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DiaChi).HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.EmailVerificationOtp)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.EmailVerificationOtpExpiry).HasColumnType("datetime");
            entity.Property(e => e.GoogleEmail)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(150);
            entity.Property(e => e.Image).HasColumnType("nvarchar(max)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsEmailVerified).HasDefaultValue(false);
            entity.Property(e => e.IsGoogleAccount).HasDefaultValue(false);
            entity.Property(e => e.LastLoginDate).HasColumnType("datetime");
            entity.Property(e => e.MatKhau).HasMaxLength(255);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.PasswordResetExpiry).HasColumnType("datetime");
            entity.Property(e => e.PasswordResetToken).HasMaxLength(255);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);

            entity.Property(e => e.EmailVerificationAttempts).HasDefaultValue(0);
        });

        modelBuilder.Entity<MigrationHistory>(entity =>
        {
            entity.HasKey(e => new { e.MigrationId, e.ContextKey }).HasName("PK_dbo.__MigrationHistory");

            entity.ToTable("__MigrationHistory");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ContextKey).HasMaxLength(300);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.MaNhaCungCap).HasName("PK_dbo.NhaCungCap");

            entity.ToTable("NhaCungCap");

            entity.Property(e => e.MaNhaCungCap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Alias).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).IsUnicode(false);
            entity.Property(e => e.IdNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID_NhanVien");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.SeoDescription).HasMaxLength(250);
            entity.Property(e => e.SeoKeywords).HasMaxLength(150);
            entity.Property(e => e.SeoTitle).HasMaxLength(150);
            entity.Property(e => e.SoDienThoai).IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasDefaultValue("");

            entity.HasOne(d => d.IdNhanVienNavigation).WithMany(p => p.NhaCungCaps)
                .HasForeignKey(d => d.IdNhanVien)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_NhaCungCap_NhanVien");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.NhanVien");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.IdChucVu, "IX_ID_ChucVu");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.Cccd)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DiaChi).HasMaxLength(500);
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.IdChucVu).HasColumnName("ID_ChucVu");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai).IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.TenHienThi)
                .HasMaxLength(150)
                .HasDefaultValue("");

            entity.HasOne(d => d.IdChucVuNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.IdChucVu)
                .HasConstraintName("FK_dbo.NhanVien_dbo.ChucVu_ID_ChucVu");
        });

        modelBuilder.Entity<PhanQuyen>(entity =>
        {
            entity.HasKey(e => new { e.IdchucVu, e.MaChucNang }).HasName("PK_dbo.PhanQuyen");

            entity.ToTable("PhanQuyen");

            entity.HasIndex(e => e.IdchucVu, "IX_IDChucVu");

            entity.HasIndex(e => e.MaChucNang, "IX_MaChucNang");

            entity.Property(e => e.IdchucVu).HasColumnName("IDChucVu");
            entity.Property(e => e.MaChucNang)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdchucVuNavigation).WithMany(p => p.PhanQuyens)
                .HasForeignKey(d => d.IdchucVu)
                .HasConstraintName("FK_dbo.PhanQuyen_dbo.ChucVu_IDChucVu");

            entity.HasOne(d => d.MaChucNangNavigation).WithMany(p => p.PhanQuyens)
                .HasForeignKey(d => d.MaChucNang)
                .HasConstraintName("FK_dbo.PhanQuyen_dbo.ChucNangQuyen_MaChucNang");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.MaSanPham).HasName("PK_dbo.Product");

            entity.ToTable("Product");

            entity.HasIndex(e => e.ProductCategoryId, "IX_ProductCategoryID");

            entity.HasIndex(e => e.SupplierId, "IX_SupplierID");

            entity.Property(e => e.MaSanPham)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Alias).HasMaxLength(500);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.GiaBan).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaNhap).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiaNiemYet).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GiamGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IdNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID_NhanVien");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ProductCategoryId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ProductCategoryID");
            entity.Property(e => e.SupplierId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SupplierID");
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasDefaultValue("");

            entity.HasOne(d => d.IdNhanVienNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdNhanVien)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_Product_NhanVien");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductCategoryId)
                .HasConstraintName("FK_dbo.Product_dbo.ProductCategory_ProductCategoryID");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Products)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_dbo.Product_dbo.NhaCungCap_SupplierID");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.MaLoaiSanPham).HasName("PK_dbo.ProductCategory");

            entity.ToTable("ProductCategory");

            entity.Property(e => e.MaLoaiSanPham)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Alias).HasMaxLength(500);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.SeoDescription).HasMaxLength(250);
            entity.Property(e => e.SeoKeywords).HasMaxLength(150);
            entity.Property(e => e.SeoTitle).HasMaxLength(150);
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasDefaultValue("");

            entity.HasOne(d => d.Category).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ProductCategory_Category");
        });

        modelBuilder.Entity<ThongKe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.ThongKe");

            entity.ToTable("ThongKe");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IdNhanVien)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID_NhanVien");
            entity.Property(e => e.ThoiGian).HasColumnType("datetime");

            entity.HasOne(d => d.IdNhanVienNavigation).WithMany(p => p.ThongKes)
                .HasForeignKey(d => d.IdNhanVien)
                .HasConstraintName("FK_ThongKe_NhanVien");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
