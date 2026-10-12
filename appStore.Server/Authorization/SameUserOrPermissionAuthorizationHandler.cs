using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;

namespace appStore.Server.Authorization
{
	public sealed class SameUserOrPermissionAuthorizationHandler : AuthorizationHandler<SameUserOrPermissionRequirement, Guid>
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context,
			SameUserOrPermissionRequirement requirement,
			Guid targetUserId
		)
		{
			if (context.User.Identity?.IsAuthenticated != true)
			{
				return Task.CompletedTask;
			}

			var currentUserIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
				?? context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

			if (Guid.TryParse(currentUserIdClaim, out var currentUserId) && currentUserId == targetUserId)
			{
				context.Succeed(requirement);
				return Task.CompletedTask;
			}

			var hasPermission = context.User
				.FindAll(AuthorizationClaimTypes.Permission)
				.Any(claim =>
					string.Equals(
						claim.Value,
						requirement.Permission,
						StringComparison.OrdinalIgnoreCase
					)
				);

			if (hasPermission)
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
