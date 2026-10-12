using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class PasswordPolicyRepository(DatabaseContext context) : Repository<PasswordPolicy, int>(context), IPasswordPolicyRepository
	{
		private const int PolicyId = 1;

		public async Task<PasswordPolicy?> GetPolicyAsync(bool asNoTracking = true, CancellationToken cancellationToken = default)
		{
			var query = DbSet.AsQueryable();

			if (asNoTracking)
			{
				query = query.AsNoTracking();
			}

			return await query.FirstOrDefaultAsync(policy => policy.Id == PolicyId, cancellationToken);
		}
	}
}
