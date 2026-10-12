namespace appStore.Server.Repositories.Interfaces
{
	public interface IUnitOfWork
	{
		Task<IExecutionTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
