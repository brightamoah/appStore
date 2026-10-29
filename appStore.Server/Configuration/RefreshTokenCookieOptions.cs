namespace appStore.Server.Configuration
{
	public sealed class RefreshTokenCookieOptions
	{
		public const string Name = "__Host-appStore-refresh";
		public const string Path = "/";
		public const bool HttpOnly = true;
		public const bool Secure = true;
		public const SameSiteMode SameSite = SameSiteMode.Lax;
	}
}