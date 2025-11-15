using System;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Models;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class GioHangTamRepository : IGioHangTamRepository
	{
		private readonly FptdrinkContext _dbContext;
		private readonly DbSet<GioHangTam> _set;

		public GioHangTamRepository(FptdrinkContext dbContext)
		{
			_dbContext = dbContext;
			_set = _dbContext.Set<GioHangTam>();
		}

		public IQueryable<GioHangTam> Query() => _set.AsQueryable();

		public async Task<IReadOnlyList<GioHangTam>> GetByCartIdAsync(string cartId, CancellationToken cancellationToken = default)
		{
			return await _set
				.Where(x => x.CartId == cartId)
				.ToListAsync(cancellationToken);
		}

		public Task AddAsync(GioHangTam entity, CancellationToken cancellationToken = default)
		{
			return _set.AddAsync(entity, cancellationToken).AsTask();
		}

		public void Update(GioHangTam entity) => _set.Update(entity);

		public void Remove(GioHangTam entity) => _set.Remove(entity);

		public async Task ClearByCartIdAsync(string cartId, CancellationToken cancellationToken = default)
		{
			var items = await _set.Where(x => x.CartId == cartId).ToListAsync(cancellationToken);
			if (items.Count > 0)
			{
				_set.RemoveRange(items);
			}
		}

		public async Task CleanupExpiredAsync(DateTime currentTimeUtc, CancellationToken cancellationToken = default)
		{
			var expiredItems = await _set.Where(x => x.ExpiryDate <= currentTimeUtc).ToListAsync(cancellationToken);
			if (expiredItems.Count > 0)
			{
				_set.RemoveRange(expiredItems);
			}
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}

