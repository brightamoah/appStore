using System.Security.Claims;

using appStore.Server.Authorization;
using appStore.Server.Errors;

using Microsoft.IdentityModel.JsonWebTokens;

namespace appStore.Server.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		extension(ClaimsPrincipal principal)
		{
			public Guid? GetUserId()
			{
				var userIdClaim =
					principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
					principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

				return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
			}

			public Guid GetRequiredUserId()
			{
				var userId = principal.GetUserId();

                return userId ?? throw AppException.Unauthorized(
	                "User identity could not be verified from the provided authentication credentials."
                );
            }

            public bool HasPermission(string permission)
			{
				return principal
					.FindAll(AuthorizationClaimTypes.Permission)
					.Any(claim => string.Equals(
						claim.Value, permission,
						StringComparison.OrdinalIgnoreCase)
					);
			}
		}
	}
}