using appStore.Server.Data;
using appStore.Server.Repositories.Interfaces;

namespace appStore.Server.Repositories
{
	public sealed class UnitOfWork(DatabaseContext context) : IUnitOfWork
	{
		public async Task<IExecutionTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
		{
			var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
			return new ExecutionTransaction(transaction);
		}

		public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await context.SaveChangesAsync(cancellationToken);
		}
	}
}
