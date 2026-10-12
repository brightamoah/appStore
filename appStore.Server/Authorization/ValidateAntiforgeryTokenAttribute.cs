using appStore.Server.Errors;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Filters;

namespace appStore.Server.Authorization
{
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public sealed class ValidateAntiforgeryTokenAttribute : Attribute, IAsyncAuthorizationFilter
	{
		public async Task OnAuthorizationAsync( AuthorizationFilterContext context )
		{
			var antiforgery = context.HttpContext.RequestServices.GetRequiredService<IAntiforgery>();

			try
			{
				await antiforgery.ValidateRequestAsync( context.HttpContext );
			}
			catch (AntiforgeryValidationException exception)
			{
				throw AppException.BadRequest(
					$"Antiforgery token validation failed: {exception.Message}",
					ErrorCodes.INVALID_REQUEST
				);
			}
		}
	}
}
