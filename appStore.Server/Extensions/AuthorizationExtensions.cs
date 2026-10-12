using appStore.Server.Authorization;

using Microsoft.AspNetCore.Authorization;

namespace appStore.Server.Extensions
{
	public static class AuthorizationExtensions
	{
		public static WebApplicationBuilder AddAuthorizationServices(this WebApplicationBuilder builder)
		{
			builder.Services.AddAuthorization();

			builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

			builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

			builder.Services.AddSingleton<IAuthorizationHandler, SameUserOrPermissionAuthorizationHandler>();

			return builder;
		}
	}
}