using System.Threading.RateLimiting;

using appStore.Server.Services.Interfaces;

using Microsoft.Extensions.Caching.Memory;

namespace appStore.Server.Services.Security
{
	public sealed class ProgressiveRateLimitTracker(IMemoryCache cache) : IProgressiveRateLimitTracker
	{
		private const int MaxAttemptsPerWindow = 5;
		private static readonly TimeSpan WindowLifespan = TimeSpan.FromMinutes(5);

		private static readonly TimeSpan[] LockoutTiers =
		[
			TimeSpan.FromMinutes(2),
			TimeSpan.FromMinutes(5),
			TimeSpan.FromMinutes(10),
			TimeSpan.FromMinutes(30),
			TimeSpan.FromHours(24)
		];

		private readonly Lock _lock = new();

		public RateLimitLease AttemptAcquire(string clientIp)
		{
			var cacheKey = $"rate_limit_auth_{clientIp}";

			lock(_lock)
			{
				var now = DateTimeOffset.UtcNow;

				var state = cache.GetOrCreate(cacheKey, entry =>
				{
					entry.SlidingExpiration = TimeSpan.FromHours(48);
					return new ClientRateLimitState
					{
						WindowStartedAt = now,
						LastAttemptAt = now
					};
				})!;


				if(state.StrikeTier > 0 && (now - state.LastAttemptAt) > TimeSpan.FromHours(1))
				{
					state.StrikeTier = 0;
					state.AttemptCount = 0;
					state.LockedUntil = null;
				}

				state.LastAttemptAt = now;


				if(state.LockedUntil.HasValue)
				{
					if(now < state.LockedUntil.Value)
					{
						var remaining = state.LockedUntil.Value - now;
						return new ProgressiveLease(isAcquired: false, retryAfter: remaining, tier: state.StrikeTier);
					}

					state.LockedUntil = null;
					state.AttemptCount = 0;
					state.WindowStartedAt = now;
				}


				if(now - state.WindowStartedAt > WindowLifespan)
				{
					state.AttemptCount = 0;
					state.WindowStartedAt = now;
				}


				state.AttemptCount++;

				if(state.AttemptCount <= MaxAttemptsPerWindow)
				{
					return new ProgressiveLease(isAcquired: true);
				}


				var tierIndex = Math.Min(state.StrikeTier, LockoutTiers.Length - 1);
				var lockoutDuration = LockoutTiers[tierIndex];

				state.LockedUntil = now + lockoutDuration;
				state.StrikeTier = Math.Min(state.StrikeTier + 1, LockoutTiers.Length - 1);

				return new ProgressiveLease(isAcquired: false, retryAfter: lockoutDuration, tier: state.StrikeTier);
			}
		}

		public void Reset(string clientIp)
		{
			var cacheKey = $"rate_limit_auth_{clientIp}";
			cache.Remove(cacheKey);
		}

		private sealed class ClientRateLimitState
		{
			public int StrikeTier { get; set; }
			public int AttemptCount { get; set; }
			public DateTimeOffset? LockedUntil { get; set; }
			public DateTimeOffset LastAttemptAt { get; set; }
			public DateTimeOffset WindowStartedAt { get; set; }
		}
	}

	internal sealed class ProgressiveLease(bool isAcquired, TimeSpan? retryAfter = null, int tier = 0) : RateLimitLease
	{
		public override bool IsAcquired => isAcquired;
		public int Tier => tier;

		public override IEnumerable<string> MetadataNames
		{
			get
			{
				if(retryAfter.HasValue)
				{
					yield return MetadataName.RetryAfter.Name;
				}
			}
		}

		public override bool TryGetMetadata(string metadataName, out object? metadata)
		{
			if(retryAfter.HasValue && metadataName == MetadataName.RetryAfter.Name)
			{
				metadata = retryAfter.Value;
				return true;
			}

			metadata = null;
			return false;
		}
	}
}