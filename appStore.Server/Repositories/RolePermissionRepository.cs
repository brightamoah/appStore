using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class RolePermissionRepository(DatabaseContext context) : IRolePermissionRepository
	{
		public async Task<IReadOnlyList<RolePermission>> GetByRoleIdAsync(
			int roleId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.RolePermissions
				.Where(rolePermission => rolePermission.RoleId == roleId)
				.Include(rolePermission => rolePermission.Role)
				.Include(rolePermission => rolePermission.Permission)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<RolePermission?> GetAsync(
			int roleId,
			int permissionId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.RolePermissions
				.Include(rolePermission => rolePermission.Role)
				.Include(rolePermission => rolePermission.Permission)
				.FirstOrDefaultAsync(
					rolePermission => rolePermission.RoleId == roleId && rolePermission.PermissionId == permissionId,
					cancellationToken
				);
		}

		public async Task<bool> IsAssignedAsync(
			int roleId,
			int permissionId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.RolePermissions.AnyAsync(
				rolePermission => rolePermission.RoleId == roleId && rolePermission.PermissionId == permissionId,
				cancellationToken
			);
		}

		public async Task<bool> IsPermissionAssignedToAnyRoleAsync(
			int permissionId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.RolePermissions.AnyAsync(
				rolePermission => rolePermission.PermissionId == permissionId,
				cancellationToken
			);
		}

		public async Task<RolePermission> AddAsync(
			RolePermission rolePermission,
			CancellationToken cancellationToken = default
		)
		{
			await context.RolePermissions.AddAsync(rolePermission, cancellationToken);
			return rolePermission;
		}

		public void Remove(RolePermission rolePermission)
		{
			context.RolePermissions.Remove(rolePermission);
		}

		public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await context.SaveChangesAsync(cancellationToken);
		}
	}
}
