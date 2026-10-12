using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Authorization;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
	public sealed class UserAuthorizationService(IUserRoleRepository userRoleRepository) : IUserAuthorizationService
	{
		public async Task<UserAuthorizationSnapshot> GetAuthorizationSnapshotAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			var roles = await userRoleRepository.GetRoleNamesByUserIdAsync(userId, cancellationToken);

			var permissions = await userRoleRepository.GetPermissionNamesByUserIdAsync(userId, cancellationToken);

			return new UserAuthorizationSnapshot(
				roles,
				permissions
			);
		}
	}
}