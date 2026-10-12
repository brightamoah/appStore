namespace appStore.Server.Repositories.Interfaces
{
	public interface IExecutionTransaction : IAsyncDisposable
	{
		Task CommitAsync(CancellationToken cancellationToken = default);
		Task RollbackAsync(CancellationToken cancellationToken = default);
	}
}
