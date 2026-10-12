using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
	{
		Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
		Task<bool> TryRotateTokenAsync(Guid existingTokenId, Guid newRefreshTokenId, DateTime revokedAt, CancellationToken cancellationToken = default);
		Task RevokeTokenAsync(Guid tokenId, DateTime revokedAt, CancellationToken cancellationToken = default);
		Task RevokeTokenFamilyAsync(Guid tokenFamilyId, DateTime revokedAt, CancellationToken cancellationToken = default);
		Task RevokeAllUserTokensAsync(Guid userId, DateTime revokedAt, CancellationToken cancellationToken = default);
	}
}
