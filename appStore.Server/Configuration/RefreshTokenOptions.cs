namespace appStore.Server.Configuration
{
	public class RefreshTokenOptions
	{
		public const string SectionName = "RefreshToken";
		public int NormalLifetimeDays { get; set; } = 1;

		public int RememberMeLifetimeDays { get; set; } = 7;


		/// <summary>
		/// When true, each successful rotation extends FamilyExpiresAt by the
		/// original lifetime, as long as the user keeps using the session.
		/// When false (default), the family hard-expires at login-time + lifetime.
		/// </summary>
		public bool SlideFamilyExpiry { get; set; } = false;

		public bool RevokeFamilyOnLogout { get; set; } = true;
	}
}