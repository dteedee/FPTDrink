using System;
using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Repositories
{
	public interface IGioHangTamRepository
	{
		IQueryable<GioHangTam> Query();
		Task<IReadOnlyList<GioHangTam>> GetByCartIdAsync(string cartId, CancellationToken cancellationToken = default);
		Task AddAsync(GioHangTam entity, CancellationToken cancellationToken = default);
		void Update(GioHangTam entity);
		void Remove(GioHangTam entity);
		Task ClearByCartIdAsync(string cartId, CancellationToken cancellationToken = default);
		Task CleanupExpiredAsync(DateTime currentTimeUtc, CancellationToken cancellationToken = default);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}

