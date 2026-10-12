using Microsoft.AspNetCore.Authorization;

namespace appStore.Server.Authorization
{
	public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
	{
		protected override Task HandleRequirementAsync( AuthorizationHandlerContext context, PermissionRequirement requirement )
		{
			if (context.User.Identity?.IsAuthenticated != true) return Task.CompletedTask;

			var hasPermission = context.User
				.FindAll( AuthorizationClaimTypes.Permission )
				.Any( claim =>
					string.Equals(
					claim.Value,
					requirement.Permission,
					StringComparison.OrdinalIgnoreCase
					)
				);

			if (hasPermission) context.Succeed( requirement );

			return Task.CompletedTask;
		}
	}
}