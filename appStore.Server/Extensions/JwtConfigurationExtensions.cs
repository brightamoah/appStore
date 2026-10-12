using appStore.Server.Configuration;

namespace appStore.Server.Extensions
{
	public static class JwtConfigurationExtensions
	{
		public static JwtOptions GetJwtOptions( this IConfiguration configuration )
		{
			return new JwtOptions
			{
				Issuer =
						configuration["JWT_ISSUER"]
						?? throw new InvalidOperationException(
							"JWT_ISSUER is not configured."
						),

				Audience =
						configuration["JWT_AUDIENCE"]
						?? throw new InvalidOperationException(
							"JWT_AUDIENCE is not configured."
						),

				Secret =
						configuration["JWT_SECRET"]
						?? throw new InvalidOperationException(
							"JWT_SECRET is not configured."
						),
				AccessTokenLifetimeMinutes =
						configuration.GetValue<int?>(
							"JWT_ACCESS_TOKEN_LIFETIME_MINUTES" )
							?? 15
			};
		}
	}
}
