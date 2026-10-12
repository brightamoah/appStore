using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore.Storage;

namespace appStore.Server.Repositories
{
	public sealed class ExecutionTransaction(IDbContextTransaction transaction) : IExecutionTransaction
	{
		public async Task CommitAsync(CancellationToken cancellationToken = default)
		{
			await transaction.CommitAsync(cancellationToken);
		}

		public async Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			await transaction.RollbackAsync(cancellationToken);
		}

		public ValueTask DisposeAsync()
		{
			return transaction.DisposeAsync();
		}
	}
}