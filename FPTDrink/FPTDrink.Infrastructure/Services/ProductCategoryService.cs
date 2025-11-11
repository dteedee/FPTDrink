using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using FPTDrink.Core.Utils;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class ProductCategoryService : IProductCategoryService
	{
		private readonly IProductCategoryRepository _repo;
		private readonly IProductRepository _productRepo;

		public ProductCategoryService(IProductCategoryRepository repo, IProductRepository productRepo)
		{
			_repo = repo;
			_productRepo = productRepo;
		}

		public async Task<IReadOnlyList<ProductCategory>> GetListAsync(string status, string? search, CancellationToken cancellationToken = default)
		{
			IQueryable<ProductCategory> q = _repo.Query();
			if (!string.IsNullOrWhiteSpace(search))
			{
				q = q.Where(x => x.Title.Contains(search));
			}
			if (status == "Index") q = q.Where(x => x.Status != 0);
			else if (status == "Trash") q = q.Where(x => x.Status == 0);
			return await q.OrderByDescending(x => x.MaLoaiSanPham).ToListAsync(cancellationToken);
		}

		public Task<ProductCategory?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
		{
			return _repo.GetByIdAsync(id, cancellationToken);
		}

		public async Task<ProductCategory> CreateAsync(ProductCategory model, CancellationToken cancellationToken = default)
		{
			var exist = await _repo.Query().AnyAsync(x => x.Title == model.Title, cancellationToken);
			if (exist) throw new InvalidOperationException("Tên loại sản phẩm đã tồn tại.");
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			var random = new Random();
			model.MaLoaiSanPham = new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
			model.CreatedBy = string.IsNullOrWhiteSpace(model.CreatedBy) ? "System" : model.CreatedBy;
			model.CreatedDate = DateTime.Now;
			model.Alias = SlugGenerator.GenerateSlug(model.Title);
			model.Status = 1;
			await _repo.AddAsync(model, cancellationToken);
			await _repo.SaveChangesAsync(cancellationToken);
			return model;
		}

		public async Task<bool> UpdateAsync(ProductCategory model, CancellationToken cancellationToken = default)
		{
			var old = await _repo.GetByIdAsync(model.MaLoaiSanPham, cancellationToken);
			if (old == null) return false;
			bool isActiveChanged = old.IsActive != model.IsActive;

			old.Modifiedby = string.IsNullOrWhiteSpace(model.Modifiedby) ? "System" : model.Modifiedby;
			old.ModifiedDate = DateTime.Now;
			old.Alias = SlugGenerator.GenerateSlug(model.Title);
			old.Title = model.Title;
			old.MoTa = model.MoTa;
			old.IsActive = model.IsActive;
			old.SeoDescription = model.SeoDescription;
			old.SeoKeywords = model.SeoKeywords;
			old.SeoTitle = model.SeoTitle;

			if (isActiveChanged)
			{
				await ApplyToProductsAsync(old.MaLoaiSanPham, p =>
				{
					if (!old.IsActive)
					{
						p.IsActive = false;
					}
					else
					{
						if (p.Status != 0) p.IsActive = true;
					}
				}, cancellationToken);
			}

			_repo.Update(old);
			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> MoveToTrashAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;
			item.IsActive = false;
			item.Status = 0;
			await ApplyToProductsAsync(item.MaLoaiSanPham, p =>
			{
				p.IsActive = false;
				p.Status = 0;
			}, cancellationToken);
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			await _productRepo.SaveChangesAsync(cancellationToken);
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
			item.IsActive = true;
			item.Status = 1;
			item.Modifiedby = "System";
			item.ModifiedDate = DateTime.Now;
			await ApplyToProductsAsync(item.MaLoaiSanPham, p =>
			{
				p.IsActive = true;
				p.Status = 1;
			}, cancellationToken);
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			await _productRepo.SaveChangesAsync(cancellationToken);
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
					obj.IsActive = true;
					obj.Status = 1;
					obj.Modifiedby = "System";
					obj.ModifiedDate = DateTime.Now;
					await ApplyToProductsAsync(obj.MaLoaiSanPham, p =>
					{
						p.IsActive = true;
						p.Status = 1;
					}, cancellationToken);
					_repo.Update(obj);
					affected++;
				}
			}
			await _repo.SaveChangesAsync(cancellationToken);
			await _productRepo.SaveChangesAsync(cancellationToken);
			return affected;
		}

		public async Task<(bool success, bool? isActive)> ToggleActiveAsync(string id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return (false, null);
			if (item.IsActive)
			{
				item.IsActive = false;
				await ApplyToProductsAsync(item.MaLoaiSanPham, p => p.IsActive = false, cancellationToken);
			}
			else
			{
				item.IsActive = true;
				await ApplyToProductsAsync(item.MaLoaiSanPham, p => { if (p.Status != 0) p.IsActive = true; }, cancellationToken);
			}
			_repo.Update(item);
			await _repo.SaveChangesAsync(cancellationToken);
			await _productRepo.SaveChangesAsync(cancellationToken);
			return (true, item.IsActive);
		}

		private async Task ApplyToProductsAsync(string categoryId, Action<Product> update, CancellationToken cancellationToken)
		{
			var products = await _productRepo.Query()
				.Where(p => p.ProductCategoryId == categoryId)
				.ToListAsync(cancellationToken);
			foreach (var p in products)
			{
				update(p);
				_productRepo.Update(p);
			}
		}
	}
}


