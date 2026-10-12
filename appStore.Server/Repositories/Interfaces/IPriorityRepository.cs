using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IPriorityRepository : IRepository<Priority, int>
	{
		Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
		Task<Priority?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	}
}
