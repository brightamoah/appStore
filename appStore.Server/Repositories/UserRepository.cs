using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class UserRepository(DatabaseContext context) : Repository<User, Guid>(context), IUserRepository
	{
		public async Task<User?> GetByEmailAsync(
			string email,
			CancellationToken cancellationToken = default
		)
		{
			var normalizedEmail = email.Trim().ToLowerInvariant();

			return await DbSet.FirstOrDefaultAsync(
				user => user.Email.ToLower() == normalizedEmail,
				cancellationToken
			);
		}

		public async Task<IReadOnlyList<User>> SearchUsersAsync(string? query, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
			}

			var trimmedQuery = query.Trim().ToLower();

			return await DbSet
				.Where(user => user.Name.ToLower().Contains(trimmedQuery) || user.Email.ToLower().Contains(trimmedQuery))
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<bool> ExistsByIdAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			return await DbSet.AnyAsync(user => user.Id == userId, cancellationToken);
		}
	}
}