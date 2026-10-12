using System.Text;
using System.Text.Json;

using appStore.Server.Configuration;
using appStore.Server.Errors;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace appStore.Server.Extensions
{
	public static class AuthenticationExtensions
	{
		public static IServiceCollection AddJwtAuthentication( this WebApplicationBuilder builder )
		{

			var jwtOptions = builder.Configuration.GetJwtOptions();
			var refreshTokenOptions = builder.Configuration.GetRefreshTokenOptions();

			builder.Services.Configure<JwtOptions>( options =>
			{
				options.Issuer = jwtOptions.Issuer;
				options.Audience = jwtOptions.Audience;
				options.Secret = jwtOptions.Secret;
				options.AccessTokenLifetimeMinutes = jwtOptions.AccessTokenLifetimeMinutes;
			} );

			builder.Services.Configure<RefreshTokenOptions>( options =>
			{
				options.NormalLifetimeDays = refreshTokenOptions.NormalLifetimeDays;
				options.RememberMeLifetimeDays = refreshTokenOptions.RememberMeLifetimeDays;
				options.SlideFamilyExpiry = refreshTokenOptions.SlideFamilyExpiry;
				options.RevokeFamilyOnLogout = refreshTokenOptions.RevokeFamilyOnLogout;
			} );

			var jsonSerializerOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			builder.Services
			.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
			.AddJwtBearer( options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidIssuer = jwtOptions.Issuer,

					ValidateAudience = true,
					ValidAudience = jwtOptions.Audience,

					ValidateLifetime = true,

					ValidateIssuerSigningKey = true,

					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes( jwtOptions.Secret )
					),

					ClockSkew = TimeSpan.Zero
				};

				options.Events = new JwtBearerEvents
				{
					OnChallenge = async context =>
					{
						context.HandleResponse();
						context.Response.StatusCode = StatusCodes.Status401Unauthorized;
						context.Response.ContentType = "application/json";

						var response = new ErrorResponse
						{
							Status = StatusCodes.Status401Unauthorized,
							Code = AuthErrorCodes.AUTHENTICATION_REQUIRED,
							Message = "Authentication is required to access this resource.",
							TraceId = context.HttpContext.TraceIdentifier
						};

						await context.Response.WriteAsync(
							JsonSerializer.Serialize( response, jsonSerializerOptions )
						);
					},

					OnForbidden = async context =>
					{
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
						context.Response.ContentType = "application/json";

						var response = new ErrorResponse
						{
							Status = StatusCodes.Status403Forbidden,
							Code = ErrorCodes.FORBIDDEN,
							Message = "You do not have permission to access this resource.",
							TraceId = context.HttpContext.TraceIdentifier
						};

						await context.Response.WriteAsync(
							JsonSerializer.Serialize( response, jsonSerializerOptions )
						);
					}
				};

			} );

			return builder.Services;
		}
	}
}