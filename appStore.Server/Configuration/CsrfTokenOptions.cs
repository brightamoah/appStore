namespace appStore.Server.Configuration
{
	public sealed class CsrfTokenOptions
	{
		public const string HeaderName = "X-CSRF-TOKEN";
		public const string CookieName = "__Host-appStore-csrf";
		public const bool HttpOnly = false;
		public const CookieSecurePolicy CookieSecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
		public const SameSiteMode SameSite = SameSiteMode.Strict;
		public const string CookiePath = "/";
	}
}