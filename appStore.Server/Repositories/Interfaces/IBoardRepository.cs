using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IBoardRepository : IRepository<BoardColumn, Guid>
	{
		Task<IReadOnlyList<BoardColumn>> GetColumnsWithTasksAsync(Guid projectId, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<BoardColumn>> GetColumnsByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
		Task<BoardColumn?> GetMatchingColumnAsync(Guid projectId, Status status, CancellationToken cancellationToken = default);
		Task<int> GetMaxOrderIndexAsync(Guid projectId, CancellationToken cancellationToken = default);
		Task<BoardColumn?> GetFirstColumnAsync(Guid projectId, CancellationToken cancellationToken = default);
	}
}
