using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IPermissionRepository : IRepository<Permission, int>
	{
		Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
		Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
	}
}