using FPTDrink.Core.Interfaces;
using FPTDrink.Infrastructure.Data;

namespace FPTDrink.Infrastructure.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly FptdrinkContext _dbContext;

		public UnitOfWork(FptdrinkContext dbContext)
		{
			_dbContext = dbContext;
		}

		public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			return _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}


