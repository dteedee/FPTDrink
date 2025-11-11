using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class NhanVienService : INhanVienService
	{
		private readonly INhanVienRepository _repo;
		private readonly IChucVuRepository _roleRepo;

		public NhanVienService(INhanVienRepository repo, IChucVuRepository roleRepo)
		{
			_repo = repo;
			_roleRepo = roleRepo;
		}

		public async Task<IReadOnlyList<NhanVien>> GetListAsync(string status, string? search, string? excludeUserId, CancellationToken cancellationToken = default)
		{
			IQueryable<NhanVien> q = _repo.Query().Include(x => x.IdChucVuNavigation);
			if (!string.IsNullOrWhiteSpace(excludeUserId))
			{
				q = q.Where(x => x.Id != excludeUserId);
			}
			if (!string.IsNullOrWhiteSpace(search))
			{
				q = q.Where(x =>
					(x.Id != null && x.Id.Contains(search)) ||
					(x.FullName != null && x.FullName.Contains(search)) ||
					(x.SoDienThoai != null && x.SoDienThoai.Contains(search)) ||
					(x.Email != null && x.Email.Contains(search)));
			}
			q = q.Where(x => x.IdChucVuNavigation == null || x.IdChucVuNavigation.TenChucVu.ToLower() != "quản lý");
			if (status == "Index") q = q.Where(x => x.Status != 0);
			else if (status == "Trash") q = q.Where(x => x.Status == 0);
			return await q.OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
		}

		public Task<NhanVien?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _repo.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		}

		public async Task<NhanVien> CreateAsync(NhanVien model, CancellationToken cancellationToken = default)
		{
			model.IsActiveAccount = true;
			model.CreatedBy = string.IsNullOrWhiteSpace(model.CreatedBy) ? "System" : model.CreatedBy;
			model.CreatedDate = DateTime.Now;
			model.Status = 1;

			if (model.NgaySinh != default)
			{
				var age = DateTime.Now.Year - model.NgaySinh.Year;
				if (model.NgaySinh > DateTime.Now.AddYears(-age)) age--;
				if (age < 18 || age > 40) throw new InvalidOperationException($"Độ tuổi phải 18-40. Tuổi: {age}");
			}

			// Không cho phép 2 người có Chức vụ "Quản lý"
			var quanLy = await _roleRepo.Query().FirstOrDefaultAsync(x => x.TenChucVu == "Quản lý", cancellationToken);
			if (quanLy != null && model.IdChucVu == quanLy.Id)
			{
				var exists = await _repo.Query().AnyAsync(x => x.IdChucVu == quanLy.Id, cancellationToken);
				if (exists) throw new InvalidOperationException("Chức vụ Quản lý đã có người đảm nhận.");
			}

			// Kiểm tra trùng tên đăng nhập
			var existUsername = await _repo.Query().AnyAsync(x => x.TenDangNhap == model.TenDangNhap, cancellationToken);
			if (existUsername) throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");

			// ID do client sinh (như code cũ) hoặc để DB/logic khác sinh
			if (string.IsNullOrWhiteSpace(model.Id))
			{
				const string chars = "0123456789";
				var random = new Random();
				model.Id = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
			}

			await _repo.AddAsync(model, cancellationToken);
			await _repo.SaveChangesAsync(cancellationToken);
			return model;
		}

		public async Task<bool> UpdateAsync(NhanVien model, CancellationToken cancellationToken = default)
		{
			var existing = await _repo.GetByIdAsync(model.Id, cancellationToken);
			if (existing == null) return false;
			// kiểm tra tuổi
			if (model.NgaySinh != default)
			{
				var age = DateTime.Now.Year - model.NgaySinh.Year;
				if (model.NgaySinh > DateTime.Now.AddYears(-age)) age--;
				if (age < 18 || age > 40) throw new InvalidOperationException($"Độ tuổi phải 18-40. Tuổi: {age}");
			}
			// kiểm tra quản lý
			var quanLy = await _roleRepo.Query().FirstOrDefaultAsync(x => x.TenChucVu == "Quản lý", cancellationToken);
			if (quanLy != null && model.IdChucVu == quanLy.Id)
			{
				var existQuanLy = await _repo.Query().AnyAsync(x => x.IdChucVu == quanLy.Id && x.Id != model.Id, cancellationToken);
				if (existQuanLy) throw new InvalidOperationException("Chức vụ 'Quản lý' đã có người đảm nhận.");
			}

			existing.FullName = model.FullName;
			existing.TenHienThi = model.TenHienThi;
			existing.Email = model.Email;
			existing.SoDienThoai = model.SoDienThoai;
			existing.DiaChi = model.DiaChi;
			existing.NgaySinh = model.NgaySinh;
			existing.GioiTinh = model.GioiTinh;
			existing.IdChucVu = model.IdChucVu;
			existing.Modifiedby = string.IsNullOrWhiteSpace(model.Modifiedby) ? "System" : model.Modifiedby;
			existing.ModifiedDate = DateTime.Now;
			_repo.Update(existing);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> MoveToTrashAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
			item.IsActiveAccount = false;
			item.Status = 0;
			item.Modifiedby = "System";
			item.ModifiedDate = DateTime.Now;
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<int> MoveToTrashBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
		{
			int affected = 0;
			foreach (var id in ids)
			{
				if (await MoveToTrashAsync(id, cancellationToken)) affected++;
			}
			return affected;
		}

		public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
		{
			return await MoveToTrashAsync(id, cancellationToken);
		}

		public async Task<int> DeleteBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
		{
			return await MoveToTrashBulkAsync(ids, cancellationToken);
		}

		public async Task<bool> UndoAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
			item.IsActiveAccount = true;
			item.Status = 1;
			item.Modifiedby = "System";
			item.ModifiedDate = DateTime.Now;
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<int> UndoBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
		{
			int affected = 0;
			foreach (var id in ids)
			{
				var item = await _repo.GetByIdAsync(id, cancellationToken);
				if (item != null)
				{
					item.IsActiveAccount = true;
					item.Status = 1;
					item.Modifiedby = "System";
					item.ModifiedDate = DateTime.Now;
					_repo.Update(item);
					affected++;
				}
			}
			await _repo.SaveChangesAsync(cancellationToken);
			return affected;
		}

		public async Task<bool> ResetPasswordAsync(string id, string newPassword, CancellationToken cancellationToken = default)
		{
			var nv = await _repo.GetByIdAsync(id, cancellationToken);
			if (nv == null) return false;
			nv.MatKhau = newPassword;
			nv.Modifiedby = "System";
			nv.ModifiedDate = DateTime.Now;
			_repo.Update(nv);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}


