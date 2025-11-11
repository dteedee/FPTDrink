using System.Linq.Expressions;
using FPTDrink.Core.Interfaces;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Repositories
{
	public class GenericRepository<T> : IRepository<T> where T : class
	{
		private readonly FptdrinkContext _dbContext;
		private readonly DbSet<T> _dbSet;

		public GenericRepository(FptdrinkContext dbContext)
		{
			_dbContext = dbContext;
			_dbSet = _dbContext.Set<T>();
		}

		public IQueryable<T> Query()
		{
			return _dbSet.AsQueryable();
		}

		public async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
		{
			return await _dbSet.FindAsync(new[] { id }, cancellationToken);
		}

		public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
		{
			IQueryable<T> query = _dbSet;
			if (predicate != null)
			{
				query = query.Where(predicate);
			}
			return await query.ToListAsync(cancellationToken);
		}

		public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
		{
			await _dbSet.AddAsync(entity, cancellationToken);
		}

		public void Update(T entity)
		{
			_dbSet.Update(entity);
		}

		public void Remove(T entity)
		{
			_dbSet.Remove(entity);
		}

		public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
		{
			if (predicate != null)
			{
				return await _dbSet.CountAsync(predicate, cancellationToken);
			}
			return await _dbSet.CountAsync(cancellationToken);
		}

		public bool Any(Expression<Func<T, bool>> predicate)
		{
			return _dbSet.Any(predicate);
		}
	}
}


