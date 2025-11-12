using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Data.Seed
{
	public static class PermissionSeeder
	{
		private static readonly (string Code, string Name)[] Features = new[]
		{
			("FPTDrink_XemDanhSach", "Xem danh sách"),
			("FPTDrink_XemChiTiet", "Xem chi tiết"),
			("FPTDrink_ThemMoi", "Thêm mới"),
			("FPTDrink_ChinhSua", "Chỉnh sửa"),
			("FPTDrink_Xoa", "Xoá"),
			("FPTDrink_ThongKe", "Xem thống kê")
		};

		public static async Task SeedAsync(FptdrinkContext db, CancellationToken ct = default)
		{
			// Seed features
			var existing = db.ChucNangQuyens.ToDictionary(x => x.MaChucNang);
			foreach (var f in Features)
			{
				if (!existing.ContainsKey(f.Code))
				{
					db.ChucNangQuyens.Add(new ChucNangQuyen
					{
						MaChucNang = f.Code,
						TenChucNang = f.Name
					});
				}
			}
			await db.SaveChangesAsync(ct);

			// Map roles
			var quanLy = db.ChucVus.FirstOrDefault(x => x.TenChucVu == "Quản lý");
			var keToan = db.ChucVus.FirstOrDefault(x => x.TenChucVu == "Kế toán");
			var thuNgan = db.ChucVus.FirstOrDefault(x => x.TenChucVu == "Thu ngân");

			if (quanLy != null)
			{
				await EnsurePermissionsAsync(db, quanLy.Id, Features.Select(f => f.Code), ct);
			}
			if (keToan != null)
			{
				await EnsurePermissionsAsync(db, keToan.Id, new[] { "FPTDrink_XemDanhSach", "FPTDrink_XemChiTiet", "FPTDrink_ThongKe" }, ct);
			}
			if (thuNgan != null)
			{
				await EnsurePermissionsAsync(db, thuNgan.Id, new[] { "FPTDrink_XemDanhSach", "FPTDrink_XemChiTiet", "FPTDrink_ChinhSua" }, ct);
			}
		}

		private static async Task EnsurePermissionsAsync(FptdrinkContext db, int roleId, IEnumerable<string> featureCodes, CancellationToken ct)
		{
			var allowedCodes = featureCodes.ToHashSet();
			var existing = await db.PhanQuyens.Where(p => p.IdchucVu == roleId).ToListAsync(ct);
			
			// Xóa các quyền không được phép
			var toRemove = existing.Where(p => !allowedCodes.Contains(p.MaChucNang)).ToList();
			if (toRemove.Count > 0)
			{
				db.PhanQuyens.RemoveRange(toRemove);
			}
			
			// Thêm các quyền còn thiếu
			var existingCodes = existing.Select(p => p.MaChucNang).ToHashSet();
			var toAdd = allowedCodes.Except(existingCodes).ToList();
			foreach (var code in toAdd)
			{
				db.PhanQuyens.Add(new PhanQuyen
				{
					IdchucVu = roleId,
					MaChucNang = code
				});
			}
			
			if (toRemove.Count > 0 || toAdd.Count > 0)
			{
				await db.SaveChangesAsync(ct);
			}
		}
	}
}


