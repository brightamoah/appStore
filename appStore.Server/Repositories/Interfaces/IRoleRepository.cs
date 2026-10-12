using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IRoleRepository : IRepository<Role, int>
	{
		Task<bool> ExistsByNameAsync(string name, int? excludeRoleId = null, CancellationToken cancellationToken = default);
		Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default);
	}
}
