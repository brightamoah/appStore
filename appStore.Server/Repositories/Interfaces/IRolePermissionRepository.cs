using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IRolePermissionRepository
	{
		Task<IReadOnlyList<RolePermission>> GetByRoleIdAsync(int roleId, CancellationToken cancellationToken = default);
		Task<RolePermission?> GetAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);
		Task<bool> IsAssignedAsync(int roleId, int permissionId, CancellationToken cancellationToken = default);
		Task<bool> IsPermissionAssignedToAnyRoleAsync(int permissionId, CancellationToken cancellationToken = default);
		Task<RolePermission> AddAsync(RolePermission rolePermission, CancellationToken cancellationToken = default);
		void Remove(RolePermission rolePermission);
		Task SaveChangesAsync(CancellationToken cancellationToken = default);
	}
}
