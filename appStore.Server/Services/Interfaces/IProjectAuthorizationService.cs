using appStore.Server.Models;

namespace appStore.Server.Services.Interfaces
{
	public interface IProjectAuthorizationService
	{
		Task<bool> HasProjectRoleAsync(
			Guid projectId,
			Guid userId,
			ProjectRole minimumRole,
			CancellationToken cancellationToken = default
		);

		Task EnsureProjectRoleAsync(
			Guid projectId,
			Guid userId,
			ProjectRole minimumRole,
			CancellationToken cancellationToken = default
		);
	}
}