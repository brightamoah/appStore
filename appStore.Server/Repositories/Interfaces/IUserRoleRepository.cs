using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IUserRoleRepository
	{
		Task<IReadOnlyList<UserRole>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<UserRole?> GetAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
		Task<bool> IsAssignedAsync(Guid userId, int roleId, CancellationToken cancellationToken = default);
		Task<bool> IsRoleAssignedToAnyUserAsync(int roleId, CancellationToken cancellationToken = default);
		Task<UserRole> AddAsync(UserRole userRole, CancellationToken cancellationToken = default);
		void Remove(UserRole userRole);
		Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<string>> GetPermissionNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
