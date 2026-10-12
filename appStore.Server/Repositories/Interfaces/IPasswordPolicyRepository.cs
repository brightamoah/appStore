using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IPasswordPolicyRepository : IRepository<PasswordPolicy, int>
	{
		Task<PasswordPolicy?> GetPolicyAsync(bool asNoTracking = true, CancellationToken cancellationToken = default);
	}
}
