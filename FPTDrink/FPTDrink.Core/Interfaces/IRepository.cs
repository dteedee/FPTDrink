using System.Linq.Expressions;

namespace FPTDrink.Core.Interfaces
{
	public interface IRepository<T> where T : class
	{
		IQueryable<T> Query();
		Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
		Task AddAsync(T entity, CancellationToken cancellationToken = default);
		void Update(T entity);
		void Remove(T entity);
		Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
		bool Any(Expression<Func<T, bool>> predicate);
	}
}


