using appStore.Server.Configuration;

namespace appStore.Server.Extensions
{
	public static class RefreshTokenConfigurationExtensions
	{
		public static RefreshTokenOptions GetRefreshTokenOptions(
		   this IConfiguration configuration )
		{
			return new RefreshTokenOptions
			{
				NormalLifetimeDays =
					configuration.GetValue<int?>(
						"REFRESH_TOKEN_NORMAL_LIFETIME_DAYS"
					) ?? 1,

				RememberMeLifetimeDays =
					configuration.GetValue<int?>(
						"REFRESH_TOKEN_REMEMBER_ME_LIFETIME_DAYS"
					) ?? 7
			};
		}
	}
}