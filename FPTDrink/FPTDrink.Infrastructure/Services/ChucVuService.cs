using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class ChucVuService : IChucVuService
	{
		private readonly IChucVuRepository _repo;
		private readonly IPhanQuyenRepository _permRepo;

		public ChucVuService(IChucVuRepository repo, IPhanQuyenRepository permRepo)
		{
			_repo = repo;
			_permRepo = permRepo;
		}

		public async Task<IReadOnlyList<ChucVu>> GetListAsync(string status, string? search, CancellationToken cancellationToken = default)
		{
			IQueryable<ChucVu> q = _repo.Query();
			if (!string.IsNullOrWhiteSpace(search))
			{
				q = q.Where(x => x.TenChucVu.Contains(search));
			}
			switch (status)
			{
				case "Index":
					q = q.Where(x => x.Status != 0);
					break;
				case "Trash":
					q = q.Where(x => x.Status == 0);
					break;
			}
			return await q.OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
		}

		public Task<ChucVu?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return _repo.GetByIdAsync(id, cancellationToken);
		}

		public async Task<ChucVu> CreateAsync(ChucVu model, CancellationToken cancellationToken = default)
		{
			model.CreatedBy = string.IsNullOrEmpty(model.CreatedBy) ? "System" : model.CreatedBy;
			model.CreatedDate = DateTime.Now;
			model.Status = 1;
			await _repo.AddAsync(model, cancellationToken);
			await _repo.SaveChangesAsync(cancellationToken);
			return model;
		}

		public async Task<bool> UpdateAsync(ChucVu model, CancellationToken cancellationToken = default)
		{
			var existing = await _repo.GetByIdAsync(model.Id, cancellationToken);
			if (existing == null) return false;
			existing.TenChucVu = model.TenChucVu;
			existing.MoTa = model.MoTa;
			existing.Modifiedby = model.Modifiedby ?? "System";
			existing.ModifiedDate = DateTime.Now;
			_repo.Update(existing);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> MoveToTrashAsync(int id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
			item.Status = 0;
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<int> MoveToTrashBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
		{
			int affected = 0;
			foreach (var id in ids)
			{
				if (await MoveToTrashAsync(id, cancellationToken)) affected++;
			}
			return affected;
		}

		public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
		{
			// Ở code cũ Delete cũng set Status=0, giữ nguyên hành vi
			return await MoveToTrashAsync(id, cancellationToken);
		}

		public async Task<int> DeleteBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
		{
			return await MoveToTrashBulkAsync(ids, cancellationToken);
		}

		public async Task<bool> UndoAsync(int id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
			item.Status = 1;
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<int> UndoBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
		{
			int affected = 0;
			foreach (var id in ids)
			{
				var item = await _repo.GetByIdAsync(id, cancellationToken);
				if (item != null)
				{
					item.Status = 1;
					_repo.Update(item);
					affected++;
				}
			}
			await _repo.SaveChangesAsync(cancellationToken);
			return affected;
		}

		public async Task<bool> TogglePermissionAsync(int chucVuId, string maChucNang, CancellationToken cancellationToken = default)
		{
			var existing = await _permRepo.FindAsync(chucVuId, maChucNang, cancellationToken);
			if (existing != null)
			{
				_permRepo.Remove(existing);
				await _permRepo.SaveChangesAsync(cancellationToken);
				return false;
			}
			var p = new PhanQuyen { IdchucVu = chucVuId, MaChucNang = maChucNang };
			await _permRepo.AddAsync(p, cancellationToken);
			await _permRepo.SaveChangesAsync(cancellationToken);
			return true;
		}
	}
}


