using appStore.Server.Services.Authorization;

namespace appStore.Server.Services.Interfaces
{
	public interface IUserAuthorizationService
	{
		Task<UserAuthorizationSnapshot> GetAuthorizationSnapshotAsync(
			Guid userId,
			CancellationToken cancellationToken = default 
		);
	}
}