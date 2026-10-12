using appStore.Server.Configuration;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
	public sealed class RefreshTokenCookieService : IRefreshTokenCookieService
	{
		public void SetRefreshTokenCookie(
			HttpResponse response,
			string refreshToken,
			DateTime expiresAt
		)
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = RefreshTokenCookieOptions.HttpOnly,
				Secure = RefreshTokenCookieOptions.Secure,
				SameSite = RefreshTokenCookieOptions.SameSite,
				Path = RefreshTokenCookieOptions.Path,
				Expires = new DateTimeOffset( expiresAt )
			};

			response.Cookies.Append(
				RefreshTokenCookieOptions.Name,
				refreshToken,
				cookieOptions
			);
		}


		public void DeleteRefreshTokenCookie( HttpResponse response )
		{
			var cookieOptions = new CookieOptions
			{
				HttpOnly = RefreshTokenCookieOptions.HttpOnly,
				Secure = RefreshTokenCookieOptions.Secure,
				SameSite = RefreshTokenCookieOptions.SameSite,
				Path = RefreshTokenCookieOptions.Path
			};

			response.Cookies.Delete(
				RefreshTokenCookieOptions.Name,
				cookieOptions
			);
		}

		public string? GetRefreshToken( HttpRequest request )
		{
			return request.Cookies.TryGetValue(
				RefreshTokenCookieOptions.Name,
				out var refreshToken
			)
				? refreshToken
				: null;
		}
	}
}