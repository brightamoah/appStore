using appStore.Server.Errors;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using appStore.Server.Services.Interfaces;
using appStore.Server.Services.Security;

namespace appStore.Server.Extensions
{
	public static class RateLimiterExtensions
	{
		public const string AuthPolicy = "auth-policy";
		public const string ApiPolicy = "api-policy";
		public const string MutationPolicy = "mutation-policy";

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public static IServiceCollection AddRateLimiting(this IServiceCollection services)
		{
			services.AddMemoryCache();

			services.AddSingleton<IProgressiveRateLimitTracker, ProgressiveRateLimitTracker>();

			services.AddRateLimiter(options =>
			{
				options.OnRejected = async (context, cancellationToken) =>
				{
					context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
					context.HttpContext.Response.ContentType = "application/json";

					int? retryAfterSeconds = null;

					if(context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
					{
						var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));

						context.HttpContext.Response.Headers.RetryAfter = seconds.ToString();

						retryAfterSeconds = seconds;
					}

					var errorResponse = new ErrorResponse
					{
						Status = StatusCodes.Status429TooManyRequests,
						Code = ErrorCodes.RATE_LIMIT_EXCEEDED,
						Message = "Too many requests. Please slow down and try again later.",
						TraceId = context.HttpContext.TraceIdentifier,
						Details = retryAfterSeconds.HasValue
							? new { retryAfterSeconds = retryAfterSeconds.Value }
							: null
					};

					await context.HttpContext.Response.WriteAsync(
						JsonSerializer.Serialize(errorResponse, JsonOptions),
						cancellationToken
					);
				};

				options.AddPolicy(AuthPolicy, httpContext =>
				{
					var tracker = httpContext.RequestServices.GetRequiredService<IProgressiveRateLimitTracker>();
					var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

					return RateLimitPartition.Get(
						partitionKey: clientIp,
						factory: _ => new ProgressiveCustomRateLimiter(tracker, clientIp)
					);
				});

				options.AddPolicy(ApiPolicy, httpContext =>
				{
					var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

					var partitionKey = !string.IsNullOrWhiteSpace(userId)
						? $"user_{userId}"
						: $"ip_{httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon"}";

					return RateLimitPartition.GetSlidingWindowLimiter(
						partitionKey: partitionKey,
						factory: _ => new SlidingWindowRateLimiterOptions
						{
							PermitLimit = 100,
							Window = TimeSpan.FromMinutes(1),
							SegmentsPerWindow = 4,
							QueueLimit = 2
						}
					);
				});

				options.AddPolicy(MutationPolicy, httpContext =>
				{
					var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

					var partitionKey = !string.IsNullOrWhiteSpace(userId)
						? $"mutation_user_{userId}"
						: $"mutation_ip_{httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon"}";


					return RateLimitPartition.GetTokenBucketLimiter(
						partitionKey: partitionKey,
						factory: _ => new TokenBucketRateLimiterOptions
						{
							TokenLimit = 40,
							TokensPerPeriod = 10,
							ReplenishmentPeriod = TimeSpan.FromSeconds(15),
							QueueLimit = 0
						}
					);
				});
			});

			return services;
		}

		private sealed class ProgressiveCustomRateLimiter(
			IProgressiveRateLimitTracker tracker,
			string clientIp
		) : RateLimiter
		{
			public override TimeSpan? IdleDuration => null;
			public override RateLimiterStatistics? GetStatistics() => null;

			protected override RateLimitLease AttemptAcquireCore(int permitCount) =>
				tracker.AttemptAcquire(clientIp);

			protected override ValueTask<RateLimitLease> AcquireAsyncCore(
				int permitCount,
				CancellationToken cancellationToken
			) => ValueTask.FromResult(tracker.AttemptAcquire(clientIp));
		}
	}
}