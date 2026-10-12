using appStore.Server.Dto.Users;

namespace appStore.Server.Services.Interfaces
{
	public interface IAuthenticationService
	{
		Task<LoginResult> LoginAsync(
			UserLoginDto loginDto,
			CancellationToken cancellationToken = default
		);

		Task<LoginResult> RefreshAsync(
			string refreshToken,
			CancellationToken cancellationToken = default
		);

		Task RevokeRefreshTokenAsync(
			string refreshToken,
			CancellationToken cancellationToken = default
		);

		Task RevokeAllUserTokensAsync(
			Guid userId,
			CancellationToken cancellationToken
		);
	}
}