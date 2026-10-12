using appStore.Server.Services.Interfaces;

using Microsoft.AspNetCore.WebUtilities;

using System.Security.Cryptography;
using System.Text;

namespace appStore.Server.Services
{
	public sealed class RefreshTokenService : IRefreshTokenService
	{
		public string GenerateToken()
		{
			const int tokenLength = 64;

			var tokenBytes = RandomNumberGenerator.GetBytes( tokenLength );

			return WebEncoders.Base64UrlEncode( tokenBytes );
		}

		public string HashToken( string token )
		{
			ArgumentException.ThrowIfNullOrWhiteSpace( token );

			var tokenBytes = Encoding.UTF8.GetBytes( token );

			var hashBytes = SHA256.HashData( tokenBytes );

			return Convert.ToBase64String( hashBytes );
		}

	}
}