using appStore.Server.Configuration;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
	public sealed class RefreshTokenCookieService : IRefreshTokenCookieService
	{
		private const string NonSecureCookieName = "appStore-refresh";

		public void SetRefreshTokenCookie(
			HttpResponse response,
			string refreshToken,
			DateTime expiresAt
		)
		{
			var isHttps = response.HttpContext.Request.IsHttps;
			var cookieName = isHttps ? RefreshTokenCookieOptions.Name : NonSecureCookieName;

			var cookieOptions = new CookieOptions
			{
				HttpOnly = RefreshTokenCookieOptions.HttpOnly,
				Secure = isHttps,
				SameSite = RefreshTokenCookieOptions.SameSite,
				Path = RefreshTokenCookieOptions.Path,
				Expires = new DateTimeOffset(expiresAt)
			};

			response.Cookies.Append(
				cookieName,
				refreshToken,
				cookieOptions
			);
		}

		public void DeleteRefreshTokenCookie(HttpResponse response)
		{
			var isHttps = response.HttpContext.Request.IsHttps;

			var cookieOptions = new CookieOptions
			{
				HttpOnly = RefreshTokenCookieOptions.HttpOnly,
				Secure = isHttps,
				SameSite = RefreshTokenCookieOptions.SameSite,
				Path = RefreshTokenCookieOptions.Path
			};

			response.Cookies.Delete(RefreshTokenCookieOptions.Name, cookieOptions);
			response.Cookies.Delete(NonSecureCookieName, cookieOptions);
		}

		public string? GetRefreshToken(HttpRequest request)
		{
			if(request.Cookies.TryGetValue(
					RefreshTokenCookieOptions.Name,
					out var secureRefreshToken
					)
				)
			{
				return secureRefreshToken;
			}

			return request.Cookies.TryGetValue(
				NonSecureCookieName,
				out var nonSecureRefreshToken
			) ? nonSecureRefreshToken : null;
		}
	}
}