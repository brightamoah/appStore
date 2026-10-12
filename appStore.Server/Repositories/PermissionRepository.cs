using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class PermissionRepository(DatabaseContext context) : Repository<Permission, int>(context), IPermissionRepository
	{
		public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			var normalizedName = name.Trim().ToLowerInvariant();

			return await DbSet.AnyAsync(
				permission => permission.Name.ToLower() == normalizedName,
				cancellationToken
			);
		}

		public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.AnyAsync(
					permission => permission.Id == id,
					cancellationToken
				);
		}
	}
}