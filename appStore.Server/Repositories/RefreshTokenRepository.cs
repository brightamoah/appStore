using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class RefreshTokenRepository(DatabaseContext context) : Repository<RefreshToken, Guid>(context), IRefreshTokenRepository
	{
		public async Task<RefreshToken?> GetByTokenHashAsync(
			string tokenHash,
			CancellationToken cancellationToken = default
		)
		{
			return await DbSet
				.AsNoTracking()
				.FirstOrDefaultAsync(
					refreshTokenEntity => refreshTokenEntity.TokenHash == tokenHash,
					cancellationToken
				);
		}

		public async Task<bool> TryRotateTokenAsync(
			Guid existingTokenId,
			Guid newRefreshTokenId,
			DateTime revokedAt,
			CancellationToken cancellationToken = default
		)
		{
			var affectedRows = await DbSet
				.Where(refreshToken => refreshToken.Id == existingTokenId && refreshToken.RevokedAt == null)
				.ExecuteUpdateAsync(
					setters => setters
						.SetProperty(refreshToken => refreshToken.RevokedAt, revokedAt)
						.SetProperty(refreshToken => refreshToken.ReplacedByTokenId, newRefreshTokenId),
					cancellationToken
				);

			return affectedRows == 1;
		}

		public async Task RevokeTokenAsync(
			Guid tokenId,
			DateTime revokedAt,
			CancellationToken cancellationToken = default
		)
		{
			await DbSet
				.Where(refreshToken => refreshToken.Id == tokenId && refreshToken.RevokedAt == null)
				.ExecuteUpdateAsync(
					setters => setters.SetProperty(refreshToken => refreshToken.RevokedAt, revokedAt),
					cancellationToken
				);
		}

		public async Task RevokeTokenFamilyAsync(
			Guid tokenFamilyId,
			DateTime revokedAt,
			CancellationToken cancellationToken = default
		)
		{
			await DbSet
				.Where(refreshToken => refreshToken.TokenFamilyId == tokenFamilyId && refreshToken.RevokedAt == null)
				.ExecuteUpdateAsync(
					setters => setters.SetProperty(refreshToken => refreshToken.RevokedAt, revokedAt),
					cancellationToken
				);
		}

		public async Task RevokeAllUserTokensAsync(
			Guid userId,
			DateTime revokedAt,
			CancellationToken cancellationToken = default
		)
		{
			await DbSet
				.Where(refreshToken => refreshToken.UserId == userId && refreshToken.RevokedAt == null)
				.ExecuteUpdateAsync(
					setters => setters.SetProperty(refreshToken => refreshToken.RevokedAt, revokedAt),
					cancellationToken
				);
		}
	}
}
