namespace appStore.Server.Services.Interfaces
{
	public interface IRefreshTokenCookieService
	{
		void SetRefreshTokenCookie(
			HttpResponse response,
			string refreshToken,
			DateTime expiresAt 
		);

		void DeleteRefreshTokenCookie( HttpResponse response );

		string? GetRefreshToken( HttpRequest request );
	}
}