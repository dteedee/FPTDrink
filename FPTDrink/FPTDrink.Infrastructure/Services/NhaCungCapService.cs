using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class NhaCungCapService : INhaCungCapService
	{
		private readonly INhaCungCapRepository _repo;

		public NhaCungCapService(INhaCungCapRepository repo)
		{
			_repo = repo;
		}

		public async Task<IReadOnlyList<NhaCungCap>> GetListAsync(string status, string? search, CancellationToken cancellationToken = default)
		{
			IQueryable<NhaCungCap> q = _repo.Query();
			if (!string.IsNullOrWhiteSpace(search))
			{
				q = q.Where(x =>
					x.MaNhaCungCap.Contains(search) ||
					x.Title.Contains(search) ||
					(x.SoDienThoai != null && x.SoDienThoai.Contains(search)) ||
					(x.Email != null && x.Email.Contains(search)));
			}
			if (status == "Index") q = q.Where(x => x.Status != 0);
			else if (status == "Trash") q = q.Where(x => x.Status == 0);
			return await q.OrderByDescending(x => x.MaNhaCungCap).ToListAsync(cancellationToken);
		}

		public Task<NhaCungCap?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _repo.GetByIdAsync(id, cancellationToken);
		}

		public async Task<NhaCungCap> CreateAsync(NhaCungCap model, CancellationToken cancellationToken = default)
		{
			var exist = await _repo.Query().AnyAsync(x => x.Title == model.Title, cancellationToken);
			if (exist) throw new InvalidOperationException("Tên nhà cung cấp đã tồn tại.");
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			model.MaNhaCungCap = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
			model.CreatedBy = string.IsNullOrWhiteSpace(model.CreatedBy) ? "System" : model.CreatedBy;
			model.CreatedDate = DateTime.Now;
			model.Alias = SlugGenerator.GenerateSlug(model.Title);
			model.Status = 1;
			await _repo.AddAsync(model, cancellationToken);
			await _repo.SaveChangesAsync(cancellationToken);
			return model;
		}

		public async Task<bool> UpdateAsync(NhaCungCap model, CancellationToken cancellationToken = default)
		{
			var existing = await _repo.GetByIdAsync(model.MaNhaCungCap, cancellationToken);
			if (existing == null) return false;
			existing.Modifiedby = string.IsNullOrWhiteSpace(model.Modifiedby) ? "System" : model.Modifiedby;
			existing.ModifiedDate = DateTime.Now;
			existing.Alias = SlugGenerator.GenerateSlug(model.Title);
			existing.Title = model.Title;
			existing.MoTa = model.MoTa;
			existing.SoDienThoai = model.SoDienThoai;
			existing.Email = model.Email;
			_repo.Update(existing);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> MoveToTrashAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
			item.Status = 0;
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

		public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
		{
			return MoveToTrashAsync(id, cancellationToken);
		}

		public Task<int> DeleteBulkAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
		{
			return MoveToTrashBulkAsync(ids, cancellationToken);
		}

		public async Task<bool> UndoAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
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
				var obj = await _repo.GetByIdAsync(id, cancellationToken);
				if (obj != null)
				{
					obj.Status = 1;
					obj.Modifiedby = "System";
					obj.ModifiedDate = DateTime.Now;
					_repo.Update(obj);
					affected++;
				}
			}
			await _repo.SaveChangesAsync(cancellationToken);
			return affected;
		}
	}
}


