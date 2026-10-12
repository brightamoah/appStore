using appStore.Server.Services.Authorization;

namespace appStore.Server.Services.Interfaces
{
	public interface IJwtTokenService
	{
		string CreateAccessToken(
			Guid userId,
			string email,
			UserAuthorizationSnapshot authorization
		);
	}
}