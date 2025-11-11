using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class ProductService : IProductService
	{
		private readonly IProductRepository _repo;

		public ProductService(IProductRepository repo)
		{
			_repo = repo;
		}

		public async Task<IReadOnlyList<Product>> GetListAsync(string status, string? search, CancellationToken cancellationToken = default)
		{
			IQueryable<Product> q = _repo.Query().Include(p => p.ProductCategory).Include(p => p.Supplier);
			if (!string.IsNullOrWhiteSpace(search))
			{
				q = q.Where(x =>
					x.MaSanPham.Contains(search) ||
					x.Title.Contains(search) ||
					(x.ProductCategory != null && x.ProductCategory.Title.Contains(search)) ||
					(x.Supplier != null && x.Supplier.Title.Contains(search)));
			}
			if (status == "Index") q = q.Where(x => x.Status != 0);
			else if (status == "Trash") q = q.Where(x => x.Status == 0);
			return await q.OrderBy(x => x.ProductCategory.Title).ToListAsync(cancellationToken);
		}

		public Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _repo.Query().Include(p => p.ProductCategory).Include(p => p.Supplier).FirstOrDefaultAsync(x => x.MaSanPham == id, cancellationToken);
		}

		public async Task<Product> CreateAsync(Product model, CancellationToken cancellationToken = default)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			model.MaSanPham = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
			model.CreatedBy = string.IsNullOrWhiteSpace(model.CreatedBy) ? "System" : model.CreatedBy;
			model.CreatedDate = DateTime.Now;
			model.Alias = SlugGenerator.GenerateSlug(model.Title);
			model.IsActive = true;
			model.Status = 1;

			if (string.IsNullOrWhiteSpace(model.ProductCategoryId)) throw new InvalidOperationException("Loại sản phẩm cần phải lựa chọn");
			if (string.IsNullOrWhiteSpace(model.SupplierId)) throw new InvalidOperationException("Nhà cung cấp cần phải lựa chọn");
			if (model.GiaNiemYet < model.GiaNhap) throw new InvalidOperationException("Giá niêm yết phải lớn hơn giá nhập");
			if (model.GiamGia.HasValue && (model.GiamGia < 0 || model.GiamGia > 100)) throw new InvalidOperationException("Tỉ lệ giảm phải từ 0 đến 100");

			if (!model.GiamGia.HasValue || model.GiamGia < 0)
			{
				model.IsSale = false;
				model.GiaBan = model.GiaNiemYet;
			}
			else if (model.GiamGia > 0)
			{
				model.IsSale = true;
				model.GiaBan = model.GiaNiemYet - (model.GiaNiemYet * (model.GiamGia.Value / 100m));
			}
			if (model.GiaBan < model.GiaNhap) throw new InvalidOperationException("Giá bán phải lớn hơn giá nhập");

			await _repo.AddAsync(model, cancellationToken);
			await _repo.SaveChangesAsync(cancellationToken);
			return model;
		}

		public async Task<bool> UpdateAsync(Product model, CancellationToken cancellationToken = default)
		{
			var existing = await _repo.GetByIdAsync(model.MaSanPham, cancellationToken);
			if (existing == null) return false;

			existing.Modifiedby = string.IsNullOrWhiteSpace(model.Modifiedby) ? "System" : model.Modifiedby;
			existing.ModifiedDate = DateTime.Now;
			existing.Alias = SlugGenerator.GenerateSlug(model.Title);
			existing.Title = model.Title;
			existing.ProductCategoryId = model.ProductCategoryId;
			existing.SupplierId = model.SupplierId;
			existing.GiaNhap = model.GiaNhap;
			existing.GiaNiemYet = model.GiaNiemYet;
			existing.GiamGia = model.GiamGia ?? 0;

			if (string.IsNullOrWhiteSpace(existing.ProductCategoryId)) throw new InvalidOperationException("Loại sản phẩm cần phải lựa chọn");
			if (string.IsNullOrWhiteSpace(existing.SupplierId)) throw new InvalidOperationException("Nhà cung cấp cần phải lựa chọn");
			if (existing.GiaNiemYet < existing.GiaNhap) throw new InvalidOperationException("Giá niêm yết phải lớn hơn giá nhập");
			if (existing.GiamGia < 0 || existing.GiamGia > 100) throw new InvalidOperationException("Tỉ lệ giảm phải từ 0 đến 100");

			if (existing.GiamGia > 0)
			{
				existing.IsSale = true;
				existing.GiaBan = existing.GiaNiemYet - (existing.GiaNiemYet * (existing.GiamGia / 100m));
			}
			else
			{
				existing.IsSale = false;
			}
			if (existing.GiaBan < existing.GiaNhap) throw new InvalidOperationException("Giá bán phải lớn hơn giá nhập");

			_repo.Update(existing);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> MoveToTrashAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
			item.IsActive = false;
			item.IsHome = false;
			item.IsHot = false;
			item.IsNew = false;
			item.IsSale = false;
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
			if (item.GiamGia > 0) item.IsSale = true;
			item.IsActive = true;
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
					if (obj.GiamGia > 0) obj.IsSale = true;
					obj.IsActive = true;
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

		public async Task<(bool success, bool? isActive)> ToggleActiveAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return (false, null);
			item.IsActive = !item.IsActive;
			item.ModifiedDate = DateTime.Now;
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			return (true, item.IsActive);
		}
	}
}


