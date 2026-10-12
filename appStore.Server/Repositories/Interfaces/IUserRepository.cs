using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IUserRepository : IRepository<User, Guid>
	{
		Task<User?> GetByEmailAsync(
			string email, 
			CancellationToken cancellationToken = default
		);
		
		Task<IReadOnlyList<User>> SearchUsersAsync(
			string? query, 
			CancellationToken cancellationToken = default
		);

		Task<bool> ExistsByIdAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		);
	}
}