using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class RoleRepository(DatabaseContext context) : Repository<Role, int>(context), IRoleRepository
	{
		public async Task<bool> ExistsByNameAsync(
			string name,
			int? excludeRoleId = null,
			CancellationToken cancellationToken = default
		)
		{
			var normalizedName = name.Trim().ToLowerInvariant();
			var query = DbSet.Where(role => role.Name.ToLower() == normalizedName);

			if (excludeRoleId.HasValue)
			{
				query = query.Where(role => role.Id != excludeRoleId.Value);
			}

			return await query.AnyAsync(cancellationToken);
		}

		public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return await DbSet.AnyAsync(role => role.Id == id, cancellationToken);
		}
	}
}
