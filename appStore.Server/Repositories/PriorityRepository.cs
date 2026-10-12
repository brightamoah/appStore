using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class PriorityRepository(DatabaseContext context) : Repository<Priority, int>(context), IPriorityRepository
	{
		public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
		{
			var trimmedName = name.Trim();
			return await DbSet.AnyAsync(
				priority => (excludeId == null || priority.Id != excludeId.Value) &&
							(EF.Functions.Collate(priority.Name, "NOCASE") == trimmedName ||
							 priority.Name.ToLower() == trimmedName.ToLower()),
				cancellationToken
			);
		}

		public async Task<Priority?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
		{
			var trimmedName = name.Trim();
			return await DbSet.FirstOrDefaultAsync(
				priority => EF.Functions.Collate(priority.Name, "NOCASE") == trimmedName ||
							priority.Name.ToLower() == trimmedName.ToLower(),
				cancellationToken
			);
		}
	}
}
