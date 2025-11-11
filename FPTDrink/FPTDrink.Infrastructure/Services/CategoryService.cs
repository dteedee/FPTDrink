using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Utils;
using FPTDrink.Core.Models;
using FPTDrink.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using FPTDrink.Infrastructure.Data;

namespace FPTDrink.Infrastructure.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly ICategoryRepository _repo;
		private readonly FptdrinkContext _dbContext;

		public CategoryService(ICategoryRepository repo, FptdrinkContext db)
		{
			_repo = repo;
			_dbContext = db;
		}

		public async Task<IReadOnlyList<Category>> GetListAsync(string status, CancellationToken cancellationToken = default)
		{
			IQueryable<Category> query = _repo.Query().OrderBy(c => c.Position);
			switch (status)
			{
				case "Index":
					query = query.Where(m => m.Status != 0);
					break;
				case "Trash":
					query = query.Where(m => m.Status == 0);
					break;
				default:
					break;
			}
			return await query.ToListAsync(cancellationToken);
		}

		public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return _repo.GetByIdAsync(id, cancellationToken);
		}

		public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
		{
			category.CreatedDate = DateTime.Now;
			category.CreatedBy = string.IsNullOrWhiteSpace(category.CreatedBy) ? "System" : category.CreatedBy;
			category.Status = 1;
			category.Alias = SlugGenerator.GenerateSlug(category.Title);

			int requestedPosition = category.Position ?? 0;
			if (requestedPosition <= 0)
			{
				int maxPosition = await _repo.Query().MaxAsync(c => (int?)c.Position, cancellationToken) ?? 0;
				category.Position = maxPosition + 1;
			}
			else
			{
				var toShift = await _repo.Query().Where(c => (c.Position ?? 0) >= requestedPosition).ToListAsync(cancellationToken);
				foreach (var cat in toShift)
				{
					cat.Position = (cat.Position ?? 0) + 1;
				}
			}

			await _repo.AddAsync(category, cancellationToken);
			await _repo.SaveChangesAsync(cancellationToken);
			return category;
		}

		public async Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken = default)
		{
			var existing = await _repo.GetByIdAsync(category.Id, cancellationToken);
			if (existing == null) return false;

			int originalPosition = existing.Position ?? 0;
			int newPosition = category.Position ?? 0;
			if (newPosition <= 0) newPosition = 1;

			if (newPosition != originalPosition)
			{
				if (newPosition < originalPosition)
				{
					var range = await _repo.Query()
						.Where(c => (c.Position ?? 0) >= newPosition && (c.Position ?? 0) < originalPosition && c.Id != category.Id)
						.ToListAsync(cancellationToken);
					foreach (var cat in range) cat.Position = (cat.Position ?? 0) + 1;
				}
				else
				{
					var range = await _repo.Query()
						.Where(c => (c.Position ?? 0) > originalPosition && (c.Position ?? 0) <= newPosition && c.Id != category.Id)
						.ToListAsync(cancellationToken);
					foreach (var cat in range) cat.Position = (cat.Position ?? 0) - 1;
				}
				existing.Position = newPosition;
			}

			existing.ModifiedDate = DateTime.Now;
			existing.Alias = SlugGenerator.GenerateSlug(category.Title);
			existing.Title = category.Title;
			existing.SeoDescription = category.SeoDescription;
			existing.SeoKeywords = category.SeoKeywords;
			existing.SeoTitle = category.SeoTitle;
			existing.IsActive = category.IsActive;
			existing.Modifiedby = category.Modifiedby;

			if (category.IsActive == false && existing.IsActive == true)
			{
				var activeProductCategories = await _dbContext.Set<ProductCategory>()
					.Where(pc => pc.IsActive == true && pc.CategoryId == existing.Id)
					.ToListAsync(cancellationToken);
				foreach (var pc in activeProductCategories)
				{
					pc.IsActive = false;
					pc.Modifiedby = "System";
					pc.ModifiedDate = DateTime.Now;

					var relatedProducts = await _dbContext.Set<Product>()
						.Where(p => p.ProductCategoryId == pc.MaLoaiSanPham && p.IsActive == true)
						.ToListAsync(cancellationToken);
					foreach (var product in relatedProducts)
					{
						product.IsActive = false;
						product.Modifiedby = "System";
						product.ModifiedDate = DateTime.Now;
					}
				}
			}

			await _repo.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<bool> MoveToTrashAsync(int id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;

			item.IsActive = false;
			item.Status = 0;
			item.Modifiedby = "System";
			item.ModifiedDate = DateTime.Now;

			var activeProductCategories = await _dbContext.Set<ProductCategory>()
				.Where(pc => pc.IsActive == true && pc.CategoryId == item.Id)
				.ToListAsync(cancellationToken);
			foreach (var pc in activeProductCategories)
			{
				pc.IsActive = false;
				pc.Modifiedby = "System";
				pc.ModifiedDate = DateTime.Now;

				var relatedProducts = await _dbContext.Set<Product>()
					.Where(p => p.ProductCategoryId == pc.MaLoaiSanPham && p.IsActive == true)
					.ToListAsync(cancellationToken);
				foreach (var product in relatedProducts)
				{
					product.IsActive = false;
					product.Modifiedby = "System";
					product.ModifiedDate = DateTime.Now;
				}
			}

			await _repo.SaveChangesAsync(cancellationToken);
			await ReorderActiveAsync(cancellationToken);
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

		public async Task<bool> UndoAsync(int id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return false;

			item.IsActive = true;
			item.Status = 1;
			item.Modifiedby = "System";
			item.ModifiedDate = DateTime.Now;
			await _repo.SaveChangesAsync(cancellationToken);
			await ReorderActiveAsync(cancellationToken);
			return true;
		}

		public async Task<int> UndoBulkAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
		{
			int affected = 0;
			foreach (var id in ids)
			{
				if (await UndoAsync(id, cancellationToken)) affected++;
			}
			return affected;
		}

		public async Task<(bool success, bool? isActive)> ToggleActiveAsync(int id, CancellationToken cancellationToken = default)
		{
			var item = await _repo.GetByIdAsync(id, cancellationToken);
			if (item == null) return (false, null);
			if (item.IsActive == true)
			{
				item.IsActive = false;

				var activeProductCategories = await _dbContext.Set<ProductCategory>()
					.Where(pc => pc.IsActive == true && pc.CategoryId == item.Id)
					.ToListAsync(cancellationToken);
				foreach (var pc in activeProductCategories)
				{
					pc.IsActive = false;
					pc.Modifiedby = "System";
					pc.ModifiedDate = DateTime.Now;

					var relatedProducts = await _dbContext.Set<Product>()
						.Where(p => p.ProductCategoryId == pc.MaLoaiSanPham && p.IsActive == true)
						.ToListAsync(cancellationToken);
					foreach (var product in relatedProducts)
					{
						product.IsActive = false;
						product.Modifiedby = "System";
						product.ModifiedDate = DateTime.Now;
					}
				}
			}
			else
			{
				item.IsActive = true;
			}
			await _repo.SaveChangesAsync(cancellationToken);
			return (true, item.IsActive);
		}

		private async Task ReorderActiveAsync(CancellationToken cancellationToken)
		{
			var remaining = await _repo.Query().Where(x => x.Status == 1).OrderBy(m => m.Position).ToListAsync(cancellationToken);
			for (int i = 0; i < remaining.Count; i++)
			{
				remaining[i].Position = i + 1;
			}
			await _repo.SaveChangesAsync(cancellationToken);
		}
	}
}


