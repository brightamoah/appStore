using appStore.Server.Errors;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace appStore.Server.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class RequireSameUserOrPermissionAttribute(
		string permission = AppPermissions.Users.Manage,
		string routeParameterName = "id"
	) : Attribute, IAsyncAuthorizationFilter
	{
		public string Permission { get; } = permission;
		public string RouteParameterName { get; } = routeParameterName;

		public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
		{
			if (context.HttpContext.User.Identity?.IsAuthenticated != true)
			{
				throw AppException.Unauthorized(
					"Authentication is required to access this resource.",
					AuthErrorCodes.AUTHENTICATION_REQUIRED
				);
			}

			if (!context.RouteData.Values.TryGetValue(RouteParameterName, out var routeVal)
				|| routeVal is null
				|| !Guid.TryParse(routeVal.ToString(), out var targetUserId))
			{
				throw AppException.BadRequest(
					$"Invalid or missing '{RouteParameterName}' route parameter.",
					ErrorCodes.INVALID_REQUEST
				);
			}

			var authorizationService = context
				.HttpContext
				.RequestServices
				.GetRequiredService<IAuthorizationService>();

			var result = await authorizationService.AuthorizeAsync(
				context.HttpContext.User,
				targetUserId,
				new SameUserOrPermissionRequirement(Permission)
			);

			if (!result.Succeeded)
			{
				throw AppException.Forbidden(
					"You do not have permission to access another user's data.",
					ErrorCodes.FORBIDDEN
				);
			}
		}
	}
}
