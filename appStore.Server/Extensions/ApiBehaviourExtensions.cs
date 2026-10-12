using appStore.Server.Configuration;
using appStore.Server.Errors;

using Microsoft.AspNetCore.Mvc;

namespace appStore.Server.Extensions
{
	public static class ApiBehaviourExtensions
	{
		public static IServiceCollection AddApiBehaviour(this WebApplicationBuilder builder)
		{
			builder.Services.Configure<ApiBehaviorOptions>(options =>
			{
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState
						.Where(error => error.Value?.Errors.Count > 0)
						.ToDictionary(
							keyValuePair => keyValuePair.Key,
							keyValuePair => keyValuePair.Value!.Errors.Select(error => error.ErrorMessage).ToArray()
						);

					var errorResponse = new ErrorResponse
					{
						Status = StatusCodes.Status400BadRequest,
						Code = ErrorCodes.VALIDATION_ERROR,
						Message = "One or more validation errors occurred.",
						TraceId = context.HttpContext.TraceIdentifier,
						Details = errors
					};

					return new BadRequestObjectResult(errorResponse);
				};
			});

			builder.Services.AddAntiforgery(options =>
			{
				options.HeaderName = CsrfTokenOptions.HeaderName;
				options.Cookie.Name = CsrfTokenOptions.CookieName;
				options.Cookie.HttpOnly = CsrfTokenOptions.HttpOnly;
				options.Cookie.SecurePolicy = CsrfTokenOptions.CookieSecurePolicy;
				options.Cookie.SameSite = CsrfTokenOptions.SameSite;
				options.Cookie.Path = CsrfTokenOptions.CookiePath;
			});

			return builder.Services;
		}
	}
}