using appStore.Server.Configuration;
using appStore.Server.Dto.Users;
using appStore.Server.Errors;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;

using Microsoft.Extensions.Options;

namespace appStore.Server.Services
{
	public sealed class AuthenticationService(
		IUserRepository userRepository,
		IRefreshTokenRepository refreshTokenRepository,
		IUnitOfWork unitOfWork,
		IPasswordHasher passwordHasher,
		IJwtTokenService jwtTokenService,
		IUserAuthorizationService authorizationService,
		IRefreshTokenService refreshTokenService,
		IOptions<RefreshTokenOptions> refreshTokenOptions,
		ILogger<AuthenticationService> logger
	) : IAuthenticationService
	{
		private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;

		private readonly string _dummyPasswordHash = "v1.pbkdf2-sha256.600000.WWZWYyNhS7Byl6NyGxZ8SQ==.hnZjKK/1e7BFPKtl3y7SCXeDSA8QDSmZNLrJoALmjx8=";

		public async Task<LoginResult> LoginAsync(
			UserLoginDto loginDto,
			CancellationToken cancellationToken = default
		)
		{
			var email = loginDto.Email.Trim().ToLowerInvariant();

			var user = await userRepository.GetByEmailAsync(
				email,
				cancellationToken
			);

			var hashToVerify = user?.PasswordHash ?? _dummyPasswordHash;

			var isPasswordValid = passwordHasher.VerifyPassword(loginDto.Password, hashToVerify);

			if (user is null || !isPasswordValid)
			{
				throw AppException.Unauthorized(
					"Invalid email or password.",
					AuthErrorCodes.INVALID_CREDENTIALS
				);
			}

			if (!user.IsUserActive)
			{
				throw AppException.Forbidden(
					"This account is deactivated.",
					ErrorCodes.USER_INACTIVE
				);
			}

			var authorization = await authorizationService.GetAuthorizationSnapshotAsync(
				user.Id,
				cancellationToken
			);

			var accessToken = jwtTokenService.CreateAccessToken(
				user.Id,
				user.Email,
				authorization
			);

			var now = DateTime.UtcNow;

			var refreshLifetime = loginDto.RememberMe
				? _refreshTokenOptions.RememberMeLifetimeDays
				: _refreshTokenOptions.NormalLifetimeDays;

			var familyExpiresAt = now.AddDays(refreshLifetime);

			var refreshTokenValue = refreshTokenService.GenerateToken();

			var refreshToken = new RefreshToken
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
				TokenHash = refreshTokenService.HashToken(refreshTokenValue),
				TokenFamilyId = Guid.NewGuid(),
				RememberMe = loginDto.RememberMe,
				CreatedAt = now,
				ExpiresAt = familyExpiresAt,
				FamilyExpiresAt = familyExpiresAt
			};

			user.LastLoginAt = now;
			user.UpdatedAt = now;

			userRepository.Update(user);
			await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
			await unitOfWork.SaveChangesAsync(cancellationToken);

			return new LoginResult(
				UserDto.FromModel(user),
				authorization.Roles,
				authorization.Permissions,
				accessToken,
				refreshTokenValue,
				refreshToken.ExpiresAt
			);
		}

		/// <summary>
		/// Rotates the refresh token. The token family has an ABSOLUTE lifetime:
		/// <see cref="RefreshToken.FamilyExpiresAt"/> is set at login and never
		/// extended, even with continuous use. This bounds the maximum session
		/// lifetime and forces re-authentication, which is the safer default.
		/// To enable sliding expiry, set <c>RefreshTokenOptions.SlideFamilyExpiry = true</c>.
		/// </summary>
		public async Task<LoginResult> RefreshAsync(
			string refreshToken,
			CancellationToken cancellationToken = default
		)
		{
			var now = DateTime.UtcNow;

			var tokenHash = refreshTokenService.HashToken(refreshToken);

			await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

			var existingToken = await LoadRefreshTokenAsync(tokenHash, cancellationToken);

			if (existingToken.RevokedAt.HasValue)
			{
				await HandleReuseDetectedAsync(
					existingToken,
					now,
					transaction,
					cancellationToken
				);
			}

			if (existingToken.ExpiresAt <= now || existingToken.FamilyExpiresAt <= now)
			{
				throw InvalidRefreshToken();
			}

			var user = await userRepository.GetByIdAsync(
				existingToken.UserId,
				cancellationToken
			);

			if (user is null || !user.IsUserActive)
			{
				throw InvalidRefreshToken();
			}

			var newRefreshTokenId = Guid.NewGuid();
			var newRefreshTokenValue = refreshTokenService.GenerateToken();

			var familyExpiresAt = existingToken.FamilyExpiresAt;
			var tokenExpiresAt = existingToken.FamilyExpiresAt;

			if (_refreshTokenOptions.SlideFamilyExpiry)
			{
				var lifetime = existingToken.RememberMe
					? _refreshTokenOptions.RememberMeLifetimeDays
					: _refreshTokenOptions.NormalLifetimeDays;

				familyExpiresAt = now.AddDays(lifetime);
				tokenExpiresAt = familyExpiresAt;
			}

			var rotated = await refreshTokenRepository.TryRotateTokenAsync(
				existingToken.Id,
				newRefreshTokenId,
				now,
				cancellationToken
			);

			if (!rotated)
			{
				await HandleConcurrentReuseAsync(
					existingToken,
					transaction,
					cancellationToken
				);
			}

			var replacementToken = new RefreshToken
			{
				Id = newRefreshTokenId,
				UserId = existingToken.UserId,
				TokenHash = refreshTokenService.HashToken(newRefreshTokenValue),
				TokenFamilyId = existingToken.TokenFamilyId,
				RememberMe = existingToken.RememberMe,
				CreatedAt = now,
				ExpiresAt = tokenExpiresAt,
				FamilyExpiresAt = familyExpiresAt
			};

			await refreshTokenRepository.AddAsync(replacementToken, cancellationToken);

			var authorization = await authorizationService.GetAuthorizationSnapshotAsync(
				user.Id,
				cancellationToken
			);

			var accessToken = jwtTokenService.CreateAccessToken(
				user.Id,
				user.Email,
				authorization
			);

			user.UpdatedAt = now;
			userRepository.Update(user);

			await unitOfWork.SaveChangesAsync(cancellationToken);

			await transaction.CommitAsync(cancellationToken);

			return new LoginResult(
				UserDto.FromModel(user),
				authorization.Roles,
				authorization.Permissions,
				accessToken,
				newRefreshTokenValue,
				replacementToken.ExpiresAt
			);
		}

		public async Task RevokeRefreshTokenAsync(
			string refreshToken,
			CancellationToken cancellationToken = default
		)
		{
			var tokenHash = refreshTokenService.HashToken(refreshToken);

			var token = await refreshTokenRepository.GetByTokenHashAsync(
				tokenHash,
				cancellationToken
			);

			if (token is null || token.RevokedAt.HasValue) return;

			var now = DateTime.UtcNow;

			if (_refreshTokenOptions.RevokeFamilyOnLogout)
			{
				await refreshTokenRepository.RevokeTokenFamilyAsync(
					token.TokenFamilyId,
					now,
					cancellationToken
				);

				logger.LogWarning(
					"Logout revoked refresh-token family {TokenFamilyId} for user {UserId}.",
					token.TokenFamilyId,
					token.UserId
				);
			}
			else
			{
				await refreshTokenRepository.RevokeTokenAsync(
					token.Id,
					now,
					cancellationToken
				);
			}
		}

		public async Task RevokeAllUserTokensAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			var now = DateTime.UtcNow;

			await refreshTokenRepository.RevokeAllUserTokensAsync(
				userId,
				now,
				cancellationToken
			);

			logger.LogWarning(
				"Revoked all refresh tokens for user {UserId}.",
				userId
			);
		}

		private static AppException InvalidRefreshToken()
		{
			return AppException.Unauthorized(
				"Invalid refresh token.",
				AuthErrorCodes.REFRESH_TOKEN_INVALID
			);
		}

		private async Task<RefreshToken> LoadRefreshTokenAsync(
			string tokenHash,
			CancellationToken cancellationToken
		)
		{
			var token = await refreshTokenRepository.GetByTokenHashAsync(
				tokenHash,
				cancellationToken
			);

			return token ?? throw InvalidRefreshToken();
		}

		private async Task HandleReuseDetectedAsync(
			RefreshToken existingToken,
			DateTime now,
			IExecutionTransaction transaction,
			CancellationToken cancellationToken
		)
		{
			if (existingToken.ReplacedByTokenId.HasValue)
			{
				await refreshTokenRepository.RevokeTokenFamilyAsync(
					existingToken.TokenFamilyId,
					now,
					cancellationToken
				);

				await transaction.CommitAsync(cancellationToken);

				logger.LogWarning(
					"Refresh token reuse detected for user {UserId}, family {TokenFamilyId}.",
					existingToken.UserId,
					existingToken.TokenFamilyId
				);
			}

			throw InvalidRefreshToken();
		}

		private async Task HandleConcurrentReuseAsync(
			RefreshToken existingToken,
			IExecutionTransaction transaction,
			CancellationToken cancellationToken
		)
		{
			await refreshTokenRepository.RevokeTokenFamilyAsync(
				existingToken.TokenFamilyId,
				DateTime.UtcNow,
				cancellationToken
			);

			await transaction.CommitAsync(cancellationToken);

			logger.LogWarning(
				"Concurrent refresh-token reuse detected for user {UserId}, family {TokenFamilyId}.",
				existingToken.UserId,
				existingToken.TokenFamilyId
			);

			throw InvalidRefreshToken();
		}
	}
}