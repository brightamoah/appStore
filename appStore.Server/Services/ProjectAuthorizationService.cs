using appStore.Server.Authorization;
using appStore.Server.Errors;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
	public class ProjectAuthorizationService(
		IProjectRepository projectRepository,
		IUserAuthorizationService userAuthorizationService
	) : IProjectAuthorizationService
	{
		public async Task<bool> HasProjectRoleAsync(
			Guid projectId,
			Guid userId,
			ProjectRole minimumRole,
			CancellationToken cancellationToken = default
		)
		{
			var authorizationSnapshot = await userAuthorizationService
					.GetAuthorizationSnapshotAsync(userId, cancellationToken);

			var isSystemAdmin = authorizationSnapshot.Permissions.Contains(AppPermissions.Users.Manage);

			if (isSystemAdmin)
			{
				return true;
			}

			var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);
			if (project != null && project.OwnerId == userId)
			{
				return true;
			}

			var membership = await projectRepository.GetMemberAsync(projectId, userId, cancellationToken);
			if (membership is null)
			{
				return false;
			}

			return membership.Role <= minimumRole;
		}

		public async Task EnsureProjectRoleAsync(
			Guid projectId,
			Guid userId,
			ProjectRole minimumRole,
			CancellationToken cancellationToken = default
		)
		{
			var hasAccess = await HasProjectRoleAsync(
				projectId,
				userId,
				minimumRole,
				cancellationToken
			);

			if (!hasAccess)
			{
				throw AppException.Forbidden(
					$"You do not have the required permissions ({minimumRole}) to perform this action in this project."
				);
			}
		}
	}
}