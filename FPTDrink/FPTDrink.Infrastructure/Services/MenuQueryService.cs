using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class MenuQueryService : IMenuQueryService
	{
		private readonly IProductCategoryRepository _pcRepo;
		private readonly INhaCungCapRepository _supRepo;
		private readonly FPTDrink.Infrastructure.Data.FptdrinkContext _db;

		public MenuQueryService(IProductCategoryRepository pcRepo, INhaCungCapRepository supRepo, FPTDrink.Infrastructure.Data.FptdrinkContext db)
		{
			_pcRepo = pcRepo;
			_supRepo = supRepo;
			_db = db;
		}

		public async Task<IReadOnlyList<Category>> GetTopMenuAsync(CancellationToken cancellationToken = default)
		{
			// Top menu theo Category (Status!=0, IsActive==true) sắp xếp Position
			return await _db.Categories.Where(x => x.Status != 0 && x.IsActive == true)
				.OrderBy(x => x.Position)
				.ToListAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<ProductCategory>> GetProductCategoriesAsync(CancellationToken cancellationToken = default)
		{
			return await _pcRepo.Query().Where(x => x.Status != 0 && x.IsActive).OrderBy(x => x.Title).ToListAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<NhaCungCap>> GetSuppliersAsync(CancellationToken cancellationToken = default)
		{
			return await _supRepo.Query().Where(x => x.Status != 0).OrderBy(x => x.Title).ToListAsync(cancellationToken);
		}
	}
}


