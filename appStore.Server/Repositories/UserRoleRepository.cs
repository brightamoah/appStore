using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class UserRoleRepository(DatabaseContext context) : IUserRoleRepository
	{
		public async Task<IReadOnlyList<UserRole>> GetByUserIdAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.UserRoles
				.Where(userRole => userRole.UserId == userId)
				.Include(userRole => userRole.Role)
				.Include(userRole => userRole.User)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<UserRole?> GetAsync(
			Guid userId,
			int roleId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.UserRoles
				.Include(userRole => userRole.User)
				.Include(userRole => userRole.Role)
				.FirstOrDefaultAsync(
					userRole => userRole.UserId == userId && userRole.RoleId == roleId,
					cancellationToken
				);
		}

		public async Task<bool> IsAssignedAsync(
			Guid userId,
			int roleId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.UserRoles.AnyAsync(
				userRole => userRole.UserId == userId && userRole.RoleId == roleId,
				cancellationToken
			);
		}

		public async Task<bool> IsRoleAssignedToAnyUserAsync(
			int roleId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.UserRoles.AnyAsync(
				userRole => userRole.RoleId == roleId,
				cancellationToken
			);
		}

		public async Task<UserRole> AddAsync(
			UserRole userRole,
			CancellationToken cancellationToken = default
		)
		{
			await context.UserRoles.AddAsync(userRole, cancellationToken);
			return userRole;
		}

		public void Remove(UserRole userRole)
		{
			context.UserRoles.Remove(userRole);
		}

		public async Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.UserRoles
				.Where(userRole => userRole.UserId == userId)
				.Select(userRole => userRole.Role.Name)
				.Distinct()
				.ToListAsync(cancellationToken);
		}

		public async Task<IReadOnlyList<string>> GetPermissionNamesByUserIdAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			return await context.UserRoles
				.Where(userRole => userRole.UserId == userId)
				.SelectMany(userRole => userRole.Role.RolePermissions)
				.Select(rolePermission => rolePermission.Permission.Name)
				.Distinct()
				.ToListAsync(cancellationToken);
		}

		public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			await context.SaveChangesAsync(cancellationToken);
		}
	}
}
