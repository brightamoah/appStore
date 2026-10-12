using System.Threading.RateLimiting;

namespace appStore.Server.Services.Interfaces
{
	public interface IProgressiveRateLimitTracker
	{
		RateLimitLease AttemptAcquire(string clientIp);
		void Reset(string clientIp);
	}
}